using System;
using System.IO;
using ProHauler.Core.Interfaces;

namespace ProHauler.Core.Services
{
    /// <summary>
    /// Helper class to initialize the coordinate projection system on application startup.
    /// </summary>
    public static class ProjectionInitializer
    {
        /// <summary>
        /// Initializes the coordinate projection with calibration data.
        /// This should be called during application startup.
        /// </summary>
        /// <param name="projection">The coordinate projection instance to initialize</param>
        /// <param name="calibrationFilePath">Path to calibration.json file (optional, defaults to assets/config/calibration.json)</param>
        /// <returns>True if initialization succeeded, false otherwise</returns>
        public static bool Initialize(ICoordinateProjection projection, string? calibrationFilePath = null)
        {
            try
            {
                // Use default path if not specified
                if (string.IsNullOrEmpty(calibrationFilePath))
                {
                    calibrationFilePath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "assets",
                        "config",
                        "calibration.json"
                    );
                }

                Console.WriteLine($"Initializing coordinate projection...");
                Console.WriteLine($"Loading calibration from: {calibrationFilePath}");

                var calibrationService = new CalibrationService(projection);

                // Load and apply calibration
                calibrationService.LoadAndApplyCalibration(calibrationFilePath);

                // Log accuracy report
                calibrationService.LogCalibrationAccuracy();

                Console.WriteLine("Coordinate projection initialized successfully\n");
                return true;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"ERROR: Calibration file not found: {ex.Message}");
                Console.WriteLine($"Please ensure calibration.json exists at: {calibrationFilePath}");
                return false;
            }
            catch (InvalidDataException ex)
            {
                Console.WriteLine($"ERROR: Invalid calibration data: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to initialize projection: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Example usage demonstrating how to use the projection system.
        /// </summary>
        public static void DemonstrateUsage()
        {
            Console.WriteLine("\n=== Coordinate Projection Demo ===\n");

            // Create projection instance
            var projection = new AffineCoordinateProjection();

            // Initialize with calibration
            if (!Initialize(projection))
            {
                Console.WriteLine("Failed to initialize projection");
                return;
            }

            // Test with some example coordinates
            Console.WriteLine("Testing coordinate transformations:");
            Console.WriteLine("-----------------------------------");

            // Test point 1: Los Angeles area
            var testPoint1 = new Models.Vector3(-87500, 0, -103000);
            var mapPos1 = projection.WorldToMap(testPoint1);
            Console.WriteLine($"World: ({testPoint1.X}, {testPoint1.Z}) -> Map: ({mapPos1.X:F0}, {mapPos1.Y:F0})");

            // Test point 2: San Francisco area
            var testPoint2 = new Models.Vector3(-91000, 0, -119000);
            var mapPos2 = projection.WorldToMap(testPoint2);
            Console.WriteLine($"World: ({testPoint2.X}, {testPoint2.Z}) -> Map: ({mapPos2.X:F0}, {mapPos2.Y:F0})");

            // Test point 3: Las Vegas area
            var testPoint3 = new Models.Vector3(-76000, 0, -96000);
            var mapPos3 = projection.WorldToMap(testPoint3);
            Console.WriteLine($"World: ({testPoint3.X}, {testPoint3.Z}) -> Map: ({mapPos3.X:F0}, {mapPos3.Y:F0})");

            // Test point 4: Midpoint between LA and Vegas
            var testPoint4 = new Models.Vector3(-81750, 0, -99500);
            var mapPos4 = projection.WorldToMap(testPoint4);
            Console.WriteLine($"World: ({testPoint4.X}, {testPoint4.Z}) -> Map: ({mapPos4.X:F0}, {mapPos4.Y:F0})");

            Console.WriteLine("\n=== Demo Complete ===\n");
        }
    }
}
