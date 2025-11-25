using System.Windows.Input;
using ProHauler.Core.Interfaces;
using ProHauler.Core.Models;

namespace ProHauler.UI.ViewModels;

/// <summary>
/// Main view model for the application window
/// </summary>
public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly IStateManager _stateManager;
    private readonly ITelemetryClient _telemetryClient;

    private PlayerState? _currentPlayerState;
    private string _connectionStatus = "Disconnected";
    private bool _isConnected;
    private bool _isDiagnosticMode;
    private double _fps;
    private string _worldCoordinates = "--";

    // Frame rate limiting
    private DateTime _lastRenderTime = DateTime.Now;
    private const int TargetFps = 60; // Target 60 FPS (exceeds 30 FPS requirement)
    private const double MinFrameTimeMs = 1000.0 / TargetFps; // ~16.67ms per frame
    private System.Diagnostics.Stopwatch _fpsStopwatch = new System.Diagnostics.Stopwatch();
    private int _frameCount = 0;

    public MainViewModel(IStateManager stateManager, ITelemetryClient telemetryClient)
    {
        _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));

        // Subscribe to events
        _stateManager.StateUpdated += OnStateUpdated;
        _telemetryClient.ConnectionStatusChanged += OnConnectionStatusChanged;

        // Initialize commands
        ToggleDiagnosticsCommand = new RelayCommand(ToggleDiagnostics);

        // Start FPS measurement
        _fpsStopwatch.Start();
    }

    #region Properties

    public PlayerState? CurrentPlayerState
    {
        get => _currentPlayerState;
        private set => SetProperty(ref _currentPlayerState, value);
    }

    public string ConnectionStatus
    {
        get => _connectionStatus;
        private set => SetProperty(ref _connectionStatus, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        private set => SetProperty(ref _isConnected, value);
    }

    public bool IsDiagnosticMode
    {
        get => _isDiagnosticMode;
        set => SetProperty(ref _isDiagnosticMode, value);
    }

    public double Fps
    {
        get => _fps;
        set => SetProperty(ref _fps, value);
    }

    public string WorldCoordinates
    {
        get => _worldCoordinates;
        private set => SetProperty(ref _worldCoordinates, value);
    }

    #endregion

    #region Commands

    public ICommand ToggleDiagnosticsCommand { get; }

    #endregion

    #region Command Implementations

    private void ToggleDiagnostics()
    {
        IsDiagnosticMode = !IsDiagnosticMode;
    }

    #endregion

    #region Event Handlers

    private void OnStateUpdated(object? sender, PlayerState state)
    {
        // Frame rate limiting: only update if enough time has passed since last render
        DateTime now = DateTime.Now;
        double timeSinceLastRender = (now - _lastRenderTime).TotalMilliseconds;

        if (timeSinceLastRender < MinFrameTimeMs)
        {
            // Skip this update to maintain target frame rate
            return;
        }

        _lastRenderTime = now;

        // Update current player state
        CurrentPlayerState = state;

        // Update world coordinates display
        try
        {
            if (_telemetryClient?.IsConnected == true)
            {
                var telemetryData = _telemetryClient.GetCurrentData();
                if (telemetryData?.Position != null)
                {
                    WorldCoordinates = $"({telemetryData.Position.X:F0}, {telemetryData.Position.Z:F0})";
                }
                else
                {
                    WorldCoordinates = "--";
                }
            }
            else
            {
                WorldCoordinates = "--";
            }
        }
        catch
        {
            WorldCoordinates = "--";
        }

        // Update FPS counter for diagnostic mode
        UpdateFpsCounter();
    }

    /// <summary>
    /// Updates the FPS counter by measuring frame rate over time
    /// </summary>
    private void UpdateFpsCounter()
    {
        _frameCount++;

        // Update FPS display every second
        if (_fpsStopwatch.ElapsedMilliseconds >= 1000)
        {
            double elapsedSeconds = _fpsStopwatch.ElapsedMilliseconds / 1000.0;
            Fps = _frameCount / elapsedSeconds;

            // Reset counters
            _frameCount = 0;
            _fpsStopwatch.Restart();
        }
    }

    private void OnConnectionStatusChanged(object? sender, string status)
    {
        ConnectionStatus = status ?? "Disconnected";
        IsConnected = _telemetryClient?.IsConnected ?? false;
    }

    #endregion

    #region IDisposable

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;

        _stateManager.StateUpdated -= OnStateUpdated;
        _telemetryClient.ConnectionStatusChanged -= OnConnectionStatusChanged;

        _disposed = true;
    }

    #endregion
}
