using System;
using System.Threading.Tasks;
using ATSLiveMap.Core.Interfaces;
using ATSLiveMap.Core.Models;
using ATSLiveMap.Core.Services;
using ATSLiveMap.Telemetry;

namespace ATSLiveMap.TestConsole
{
    class Program
    {
        private static ITelemetryClient? _telemetryClient;
        private static StateManager? _stateManager;
        private static int _updateCount = 0;
        private static DateTime _lastUpdate = DateTime.Now;

        static async Task Main(string[] args)
        {
            // Check if user wants diagnostic mode
            if (args.Length > 0 && args[0].ToLower() == "--check")
            {
                SharedMemoryChecker.CheckSharedMemory();
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("===========================================");
            Console.WriteLine("  ATS Live Map - Telemetry Test Console");
            Console.WriteLine("===========================================");
            Console.WriteLine();
            Console.WriteLine("This test will:");
            Console.WriteLine("  1. Connect to ATS telemetry shared memory");
            Console.WriteLine("  2. Display live telemetry data");
            Console.WriteLine("  3. Show state manager with smoothing");
            Console.WriteLine();
            Console.WriteLine("Make sure:");
            Console.WriteLine("  - ATS is running");
            Console.WriteLine("  - Telemetry plugin is installed");
            Console.WriteLine("  - You're in-game (not in menu)");
            Console.WriteLine();
            Console.WriteLine("TIP: Run with --check flag to diagnose issues");
            Console.WriteLine("Press Ctrl+C to exit");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            // Set up Ctrl+C handler
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("\n\nShutting down...");
                _telemetryClient?.StopAsync().Wait();
                Environment.Exit(0);
            };

            try
            {
                // Create coordinate projection (with dummy calibration for now)
                var projection = new AffineCoordinateProjection();
                
                // Create simple calibration (identity transform for testing)
                var calibrationPoints = new System.Collections.Generic.List<CalibrationPoint>
                {
                    new CalibrationPoint 
                    { 
                        LocationName = "Origin",
                        WorldPosition = new Vector3(0, 0, 0),
                        MapPixelPosition = new System.Windows.Point(0, 0)
                    },
                    new CalibrationPoint 
                    { 
                        LocationName = "Test1",
                        WorldPosition = new Vector3(1000, 0, 0),
                        MapPixelPosition = new System.Windows.Point(100, 0)
                    },
                    new CalibrationPoint 
                    { 
                        LocationName = "Test2",
                        WorldPosition = new Vector3(0, 0, 1000),
                        MapPixelPosition = new System.Windows.Point(0, 100)
                    }
                };
                projection.Calibrate(calibrationPoints);

                // Create state manager with smoothing
                _stateManager = new StateManager(projection);

                // Subscribe to state updates
                _stateManager.StateUpdated += OnStateUpdated;
                _stateManager.ConnectionStatusChanged += OnConnectionStatusChanged;

                // Create telemetry client
                // Try HTTP client first (for Funbit telemetry server)
                Console.WriteLine("Trying HTTP telemetry client (Funbit server)...");
                _telemetryClient = new HttpTelemetryClient();
                
                // Alternative: Use shared memory client
                // _telemetryClient = new SharedMemoryTelemetryClient();

                // Subscribe to telemetry events
                _telemetryClient.DataUpdated += OnTelemetryDataUpdated;
                _telemetryClient.ConnectionStatusChanged += OnTelemetryConnectionStatusChanged;

                // Start the telemetry client
                Console.WriteLine("Starting telemetry client...");
                await _telemetryClient.StartAsync();

                // Keep running until Ctrl+C
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nFATAL ERROR: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        private static void OnTelemetryConnectionStatusChanged(object? sender, string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[TELEMETRY] {message}");
            Console.ResetColor();
        }

        private static void OnConnectionStatusChanged(object? sender, ConnectionStatusChangedEventArgs e)
        {
            var color = e.Status switch
            {
                ConnectionStatus.Connected => ConsoleColor.Green,
                ConnectionStatus.Connecting => ConsoleColor.Yellow,
                ConnectionStatus.Disconnected => ConsoleColor.Gray,
                ConnectionStatus.Error => ConsoleColor.Red,
                _ => ConsoleColor.White
            };

            Console.ForegroundColor = color;
            Console.WriteLine($"[STATE MGR] {e.Message}");
            Console.ResetColor();
        }

        private static void OnTelemetryDataUpdated(object? sender, TelemetryData data)
        {
            _updateCount++;

            // Update state manager with new telemetry
            _stateManager?.UpdateFromTelemetry(data);

            // Display update every 1 second (to avoid flooding console)
            var now = DateTime.Now;
            if ((now - _lastUpdate).TotalSeconds >= 1.0)
            {
                DisplayTelemetryData(data);
                _lastUpdate = now;
            }
        }

        private static void OnStateUpdated(object? sender, PlayerState state)
        {
            // State updates happen frequently, we'll display them with telemetry data
        }

        private static void DisplayTelemetryData(TelemetryData data)
        {
            Console.Clear();
            Console.WriteLine("===========================================");
            Console.WriteLine("  ATS Live Map - Telemetry Test Console");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            // Connection info
            Console.ForegroundColor = data.IsConnected ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"Status: {(data.IsConnected ? "CONNECTED" : "DISCONNECTED")}");
            Console.ResetColor();
            Console.WriteLine($"Game: {data.GameName}");
            Console.WriteLine($"Paused: {data.IsPaused}");
            Console.WriteLine($"Updates received: {_updateCount}");
            Console.WriteLine($"Update rate: ~{_updateCount / (DateTime.Now - _lastUpdate.AddSeconds(-1)).TotalSeconds:F1} Hz");
            Console.WriteLine();

            // Raw telemetry data
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- RAW TELEMETRY DATA ---");
            Console.ResetColor();
            Console.WriteLine($"Position (World):");
            Console.WriteLine($"  X: {data.Position.X,12:F2} m");
            Console.WriteLine($"  Y: {data.Position.Y,12:F2} m");
            Console.WriteLine($"  Z: {data.Position.Z,12:F2} m");
            Console.WriteLine();
            Console.WriteLine($"Orientation:");
            Console.WriteLine($"  Heading: {data.Heading,10:F4} rad ({RadiansToDegrees(data.Heading),6:F1}°)");
            Console.WriteLine($"  Pitch:   {data.Pitch,10:F4} rad ({RadiansToDegrees(data.Pitch),6:F1}°)");
            Console.WriteLine($"  Roll:    {data.Roll,10:F4} rad ({RadiansToDegrees(data.Roll),6:F1}°)");
            Console.WriteLine();
            Console.WriteLine($"Speed: {data.Speed:F2} m/s ({data.Speed * 2.23694:F1} mph)");
            Console.WriteLine($"Timestamp: {data.Timestamp:HH:mm:ss.fff}");
            Console.WriteLine();

            // State manager data (with smoothing)
            if (_stateManager != null)
            {
                var state = _stateManager.CurrentState;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("--- STATE MANAGER (WITH SMOOTHING) ---");
                Console.ResetColor();
                Console.WriteLine($"Map Position (Raw):");
                Console.WriteLine($"  X: {state.MapPosition.X,12:F2} px");
                Console.WriteLine($"  Y: {state.MapPosition.Y,12:F2} px");
                Console.WriteLine();
                Console.WriteLine($"Map Position (Smoothed):");
                Console.WriteLine($"  X: {state.SmoothedMapPosition.X,12:F2} px");
                Console.WriteLine($"  Y: {state.SmoothedMapPosition.Y,12:F2} px");
                Console.WriteLine();
                Console.WriteLine($"Heading (Raw):     {state.Heading,10:F4} rad ({RadiansToDegrees(state.Heading),6:F1}°)");
                Console.WriteLine($"Heading (Smoothed): {state.SmoothedHeading,10:F4} rad ({RadiansToDegrees(state.SmoothedHeading),6:F1}°)");
                Console.WriteLine();
                Console.WriteLine($"Speed: {state.Speed:F2} m/s ({state.Speed * 2.23694:F1} mph)");
                
                // Calculate smoothing difference
                var posDiff = Math.Sqrt(
                    Math.Pow(state.MapPosition.X - state.SmoothedMapPosition.X, 2) +
                    Math.Pow(state.MapPosition.Y - state.SmoothedMapPosition.Y, 2));
                var headingDiff = Math.Abs(state.Heading - state.SmoothedHeading);
                
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Smoothing Delta:");
                Console.WriteLine($"  Position: {posDiff:F2} px");
                Console.WriteLine($"  Heading:  {headingDiff:F4} rad ({RadiansToDegrees(headingDiff):F2}°)");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.WriteLine("===========================================");
            Console.WriteLine("Press Ctrl+C to exit");
        }

        private static double RadiansToDegrees(float radians)
        {
            return radians * (180.0 / Math.PI);
        }
    }
}
