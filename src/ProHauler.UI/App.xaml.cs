using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ProHauler.Core.Interfaces;
using ProHauler.Core.Services;
using ProHauler.Core.Data;
using ProHauler.Telemetry;
using ProHauler.UI.ViewModels;
using System.Linq;

namespace ProHauler.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public ServiceProvider? Services => _serviceProvider;

    public App()
    {
        // Set up global exception handlers
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Wire up telemetry client to state manager, performance calculator, and trip detection
        var telemetryClient = _serviceProvider.GetRequiredService<ITelemetryClient>();
        var stateManager = _serviceProvider.GetRequiredService<IStateManager>();
        var performanceCalculator = _serviceProvider.GetRequiredService<IPerformanceCalculator>();
        var tripDetectionService = _serviceProvider.GetRequiredService<ITripDetectionService>();

        // Subscribe state manager, performance calculator, and trip detection to telemetry updates
        telemetryClient.DataUpdated += (sender, data) =>
        {
            Dispatcher.Invoke(() =>
            {
                stateManager.UpdateFromTelemetry(data);
                performanceCalculator.UpdateFromTelemetry(data);
                tripDetectionService.UpdateFromTelemetry(data);
            });
        };

        // Load calibration and initialize coordinate projection
        var coordinateProjection = _serviceProvider.GetRequiredService<ICoordinateProjection>();
        try
        {
            // Load calibration points from calibration.json
            var calibrationPoints = LoadCalibrationPoints();

            if (calibrationPoints != null && calibrationPoints.Count >= 3)
            {
                // Set map bounds to match the ultra high-res map
                coordinateProjection.SetMapBounds(16640, 20736);
                coordinateProjection.Calibrate(calibrationPoints);

                System.Diagnostics.Debug.WriteLine($"Calibration loaded successfully with {calibrationPoints.Count} points");

                // Log each calibration point
                string calibInfo = $"Loaded {calibrationPoints.Count} calibration points:\n";
                foreach (var point in calibrationPoints)
                {
                    System.Diagnostics.Debug.WriteLine($"  {point.LocationName}: World({point.WorldPosition.X}, {point.WorldPosition.Z}) -> Map({point.MapPixelPosition.X}, {point.MapPixelPosition.Y})");
                    calibInfo += $"{point.LocationName}: W({point.WorldPosition.X},{point.WorldPosition.Z}) -> M({point.MapPixelPosition.X},{point.MapPixelPosition.Y})\n";
                }

                MessageBox.Show(calibInfo, "Calibration Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Warning: No calibration points found");
                MessageBox.Show("No calibration points found!", "Calibration Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Calibration loading error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            MessageBox.Show(
                $"Failed to load calibration:\n\n{ex.Message}\n\nPlayer position may be incorrect.",
                "Calibration Warning",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        // Try to start telemetry server if not running
        TryStartTelemetryServer();

        // Start telemetry client in background
        _ = Task.Run(async () =>
        {
            try
            {
                await telemetryClient.StartAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't crash the app
                System.Diagnostics.Debug.WriteLine($"Telemetry client error: {ex.Message}");
            }
        });

        // Create and show performance window as main window
        var performanceWindow = _serviceProvider.GetRequiredService<Views.PerformanceWindow>();
        performanceWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register configuration
        var configuration = new ProHauler.Core.Models.AppConfiguration();
        services.AddSingleton(configuration);

        // Register Core services
        services.AddSingleton<ICoordinateProjection, AffineCoordinateProjection>();

        // Register map service with error handling
        services.AddSingleton<IMapService>(sp =>
        {
            try
            {
                return new SingleImageMapService(configuration);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Map service initialization warning: {ex.Message}");
                // Return a service that will handle missing map gracefully
                return new SingleImageMapService(configuration);
            }
        });

        services.AddSingleton<IStateManager>(sp =>
        {
            var projection = sp.GetRequiredService<ICoordinateProjection>();
            return new StateManager(
                projection,
                configuration.PositionSmoothingFactor,
                configuration.HeadingSmoothingFactor);
        });

        // Register Telemetry services
        // Use HttpTelemetryClient for Funbit's telemetry server
        services.AddSingleton<ITelemetryClient, HttpTelemetryClient>();

        // Register Performance Tracking services
        services.AddSingleton<IPerformanceCalculator, PerformanceCalculator>();

        // Register Trip Persistence services
        // Initialize database path
        var databasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ProHauler",
            "trips.db");

        // Initialize database (will be called during startup)
        _ = Task.Run(async () =>
        {
            try
            {
                await ProHauler.Core.Data.DatabaseInitializer.InitializeDatabaseAsync(databasePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
            }
        });

        services.AddSingleton<ITripRepository>(sp => new TripRepository(databasePath));
        services.AddSingleton<ITripDetectionService, TripDetectionService>();

        // Register ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<PerformanceViewModel>();

        // Register Views - use transient so it's created fresh each time
        services.AddTransient<MainWindow>(sp =>
        {
            var viewModel = sp.GetRequiredService<MainViewModel>();
            return new MainWindow(viewModel);
        });
        services.AddTransient<Views.PerformanceWindow>();
    }

    private void TryStartTelemetryServer()
    {
        try
        {
            // Check if telemetry server is already running
            using var client = new System.Net.Http.HttpClient();
            client.Timeout = TimeSpan.FromSeconds(2);
            var response = client.GetAsync("http://localhost:25555").Result;

            // Server is already running
            System.Diagnostics.Debug.WriteLine("Telemetry server already running");
            return;
        }
        catch
        {
            // Server not running, try to start it
            System.Diagnostics.Debug.WriteLine("Telemetry server not detected");

            // For now, just show a message. To fully implement this, you'd need to:
            // 1. Bundle the telemetry server files with your app
            // 2. Start it as a process here
            // 3. Track the process and kill it on exit

            MessageBox.Show(
                "Telemetry server not detected.\n\n" +
                "Please ensure the ETS2/ATS telemetry server is running on port 25555.\n\n" +
                "The application will continue, but performance tracking requires the telemetry server.",
                "Telemetry Server",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        // End current trip if active before closing
        if (_serviceProvider != null)
        {
            var tripDetectionService = _serviceProvider.GetService<ITripDetectionService>();
            if (tripDetectionService != null && tripDetectionService.IsTripActive)
            {
                try
                {
                    await tripDetectionService.EndCurrentTripAsync();
                    System.Diagnostics.Debug.WriteLine("Trip saved on application exit");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to save trip on exit: {ex.Message}");
                }
            }
        }

        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        MessageBox.Show(
            $"An unexpected error occurred:\n\n{exception?.Message}\n\nThe application will continue running.",
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe application will continue running.",
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved();
        System.Diagnostics.Debug.WriteLine($"Unobserved task exception: {e.Exception.Message}");
    }

    public ITelemetryClient? GetTelemetryClient()
    {
        return _serviceProvider?.GetService<ITelemetryClient>();
    }

    private List<ProHauler.Core.Models.CalibrationPoint> LoadCalibrationPoints()
    {
        // Try multiple possible paths
        string[] possiblePaths = new[]
        {
            "assets/config/calibration.json",
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/config/calibration.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "assets/config/calibration.json")
        };

        string? calibrationPath = null;
        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                calibrationPath = path;
                break;
            }
        }

        if (calibrationPath == null)
        {
            throw new FileNotFoundException($"Calibration file not found in any of these locations:\n{string.Join("\n", possiblePaths)}");
        }

        string json = File.ReadAllText(calibrationPath);

        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var calibrationData = System.Text.Json.JsonSerializer.Deserialize<CalibrationData>(json, options);

        if (calibrationData?.ReferencePoints == null)
        {
            return new List<ProHauler.Core.Models.CalibrationPoint>();
        }

        // Convert from JSON format to CalibrationPoint objects
        var points = new List<ProHauler.Core.Models.CalibrationPoint>();
        foreach (var point in calibrationData.ReferencePoints)
        {
            points.Add(new ProHauler.Core.Models.CalibrationPoint
            {
                LocationName = point.Name ?? "Unknown",
                WorldPosition = new ProHauler.Core.Models.Vector3(
                    point.WorldPosition.X,
                    point.WorldPosition.Y,
                    point.WorldPosition.Z
                ),
                MapPixelPosition = new System.Windows.Point(
                    point.MapPixelPosition.X,
                    point.MapPixelPosition.Y
                )
            });
        }

        return points;
    }

    private class CalibrationData
    {
        public List<CalibrationPointJson>? ReferencePoints { get; set; }
    }

    private class CalibrationPointJson
    {
        public string? Name { get; set; }
        public Vector3Json WorldPosition { get; set; } = new Vector3Json();
        public PointJson MapPixelPosition { get; set; } = new PointJson();
    }

    private class Vector3Json
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    private class PointJson
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}

