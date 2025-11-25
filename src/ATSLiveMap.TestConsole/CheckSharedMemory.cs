using System;
using System.IO.MemoryMappedFiles;

namespace ATSLiveMap.TestConsole
{
    public static class SharedMemoryChecker
    {
        public static void CheckSharedMemory()
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  Shared Memory Diagnostic Tool");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            string[] memoryMapNames = new[]
            {
                "Local\\SCSTelemetry",           // Standard SCS telemetry
                "Local\\SimTelemetryETS2",       // Alternative name
                "Local\\SimTelemetryATS",        // Alternative name
                "SCSTelemetry",                  // Without Local prefix
            };

            Console.WriteLine("Checking for shared memory regions...");
            Console.WriteLine();

            bool foundAny = false;

            foreach (var name in memoryMapNames)
            {
                try
                {
                    using (var mmf = MemoryMappedFile.OpenExisting(name))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"✓ FOUND: {name}");
                        Console.ResetColor();

                        // Try to read some data
                        try
                        {
                            using (var accessor = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read))
                            {
                                Console.WriteLine($"  Size: {accessor.Capacity} bytes");
                                
                                // Read first 100 bytes to see if there's data
                                byte[] buffer = new byte[Math.Min(100, (int)accessor.Capacity)];
                                accessor.ReadArray(0, buffer, 0, buffer.Length);
                                
                                string preview = System.Text.Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                                if (!string.IsNullOrWhiteSpace(preview))
                                {
                                    Console.WriteLine($"  Preview: {preview.Substring(0, Math.Min(50, preview.Length))}...");
                                }
                                else
                                {
                                    Console.WriteLine("  (No readable data yet)");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  Could not read data: {ex.Message}");
                        }

                        foundAny = true;
                        Console.WriteLine();
                    }
                }
                catch (FileNotFoundException)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"✗ Not found: {name}");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"? Error checking {name}: {ex.Message}");
                    Console.ResetColor();
                }
            }

            Console.WriteLine();
            Console.WriteLine("===========================================");

            if (!foundAny)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ NO SHARED MEMORY FOUND");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("This means:");
                Console.WriteLine("  1. ATS is not running, OR");
                Console.WriteLine("  2. Telemetry plugin is not installed, OR");
                Console.WriteLine("  3. Plugin failed to load");
                Console.WriteLine();
                Console.WriteLine("Next steps:");
                Console.WriteLine("  1. Run check-telemetry-plugin.bat to verify installation");
                Console.WriteLine("  2. Check Documents\\American Truck Simulator\\game.log.txt");
                Console.WriteLine("  3. Make sure you're IN-GAME (not in menu)");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Shared memory found! Telemetry should work.");
                Console.ResetColor();
            }

            Console.WriteLine("===========================================");
        }
    }
}
