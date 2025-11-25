using System;
using System.Windows;
using ProHauler.Core.Interfaces;
using ProHauler.Core.Models;

namespace ProHauler.Core.Services
{
    /// <summary>
    /// Manages player state by receiving telemetry updates, applying coordinate projection,
    /// and maintaining both raw and smoothed values for rendering.
    /// </summary>
    public class StateManager : IStateManager
    {
        private readonly ICoordinateProjection _coordinateProjection;
        private readonly LinearSmoother _smoother;
        private PlayerState _currentState;
        private ConnectionStatus _status;
        private bool _isFirstUpdate;

        /// <summary>
        /// Gets the current player state with both raw and smoothed values.
        /// </summary>
        public PlayerState CurrentState => _currentState;

        /// <summary>
        /// Gets the current connection status.
        /// </summary>
        public ConnectionStatus Status => _status;

        /// <summary>
        /// Event fired when player state changes.
        /// </summary>
        public event EventHandler<PlayerState>? StateUpdated;

        /// <summary>
        /// Event fired when connection status changes.
        /// </summary>
        public event EventHandler<ConnectionStatusChangedEventArgs>? ConnectionStatusChanged;

        /// <summary>
        /// Initializes a new instance of the StateManager class.
        /// </summary>
        /// <param name="coordinateProjection">The coordinate projection service for converting world to map coordinates.</param>
        /// <param name="positionSmoothingFactor">Smoothing factor for position (default 0.3).</param>
        /// <param name="headingSmoothingFactor">Smoothing factor for heading (default 0.5).</param>
        public StateManager(
            ICoordinateProjection coordinateProjection,
            float positionSmoothingFactor = 0.3f,
            float headingSmoothingFactor = 0.5f)
        {
            _coordinateProjection = coordinateProjection ?? throw new ArgumentNullException(nameof(coordinateProjection));
            _smoother = new LinearSmoother(positionSmoothingFactor, headingSmoothingFactor);
            _currentState = new PlayerState
            {
                MapPosition = new Point(0, 0),
                Heading = 0,
                Speed = 0,
                Timestamp = DateTime.Now,
                SmoothedMapPosition = new Point(0, 0),
                SmoothedHeading = 0
            };
            _status = ConnectionStatus.Disconnected;
            _isFirstUpdate = true;
        }

        /// <summary>
        /// Updates the player state from new telemetry data.
        /// Applies coordinate projection to convert world position to map position.
        /// Applies smoothing to reduce visual jitter.
        /// </summary>
        /// <param name="data">The telemetry data received from the game.</param>
        public void UpdateFromTelemetry(TelemetryData data)
        {
            if (data == null)
            {
                return;
            }

            // Update connection status based on telemetry data
            UpdateConnectionStatus(data);

            // Convert world position to map position using coordinate projection
            Point mapPosition;
            try
            {
                mapPosition = _coordinateProjection.WorldToMap(data.Position);
                System.Diagnostics.Debug.WriteLine($"Position: World({data.Position.X:F0}, {data.Position.Z:F0}) -> Map({mapPosition.X:F0}, {mapPosition.Y:F0})");
            }
            catch (InvalidOperationException ex)
            {
                // Projection not calibrated yet - use default position
                System.Diagnostics.Debug.WriteLine($"Projection error: {ex.Message}");
                mapPosition = new Point(0, 0);
            }

            // Apply smoothing to position and heading
            Point smoothedPosition;
            float smoothedHeading;

            if (_isFirstUpdate)
            {
                // On first update, use raw values without smoothing
                smoothedPosition = mapPosition;
                smoothedHeading = data.Heading;
                _isFirstUpdate = false;
            }
            else
            {
                // Apply linear interpolation smoothing
                smoothedPosition = _smoother.SmoothPosition(
                    _currentState.SmoothedMapPosition,
                    mapPosition);

                smoothedHeading = _smoother.SmoothHeading(
                    _currentState.SmoothedHeading,
                    data.Heading);
            }

            // Create new player state with both raw and smoothed values
            var newState = new PlayerState
            {
                MapPosition = mapPosition,
                Heading = data.Heading,
                Speed = data.Speed,
                Timestamp = data.Timestamp,
                SmoothedMapPosition = smoothedPosition,
                SmoothedHeading = smoothedHeading
            };

            // Update current state
            _currentState = newState;

            // Emit state updated event
            OnStateUpdated(newState);
        }

        /// <summary>
        /// Updates the connection status based on telemetry data.
        /// Emits ConnectionStatusChanged event if status changes.
        /// </summary>
        /// <param name="data">The telemetry data.</param>
        private void UpdateConnectionStatus(TelemetryData data)
        {
            ConnectionStatus newStatus;
            string statusMessage;

            if (data.IsConnected)
            {
                newStatus = ConnectionStatus.Connected;
                statusMessage = $"Connected to {data.GameName}";
            }
            else
            {
                newStatus = ConnectionStatus.Disconnected;
                statusMessage = "Disconnected from game";
            }

            // Only emit event if status actually changed
            if (newStatus != _status)
            {
                _status = newStatus;
                OnConnectionStatusChanged(newStatus, statusMessage);
            }
        }

        /// <summary>
        /// Sets the connection status to Connecting state.
        /// This should be called when attempting to establish connection.
        /// </summary>
        public void SetConnecting()
        {
            if (_status != ConnectionStatus.Connecting)
            {
                _status = ConnectionStatus.Connecting;
                OnConnectionStatusChanged(ConnectionStatus.Connecting, "Connecting to ATS...");
            }
        }

        /// <summary>
        /// Sets the connection status to Error state with a specific error message.
        /// </summary>
        /// <param name="errorMessage">The error message to display.</param>
        public void SetError(string errorMessage)
        {
            _status = ConnectionStatus.Error;
            OnConnectionStatusChanged(ConnectionStatus.Error, $"Error: {errorMessage}");
        }

        /// <summary>
        /// Sets the connection status to Disconnected state.
        /// This should be called when connection is lost or when waiting for game to start.
        /// </summary>
        /// <param name="message">Optional custom message (defaults to "Waiting for ATS to start...").</param>
        public void SetDisconnected(string message = "Waiting for ATS to start...")
        {
            if (_status != ConnectionStatus.Disconnected)
            {
                _status = ConnectionStatus.Disconnected;
                OnConnectionStatusChanged(ConnectionStatus.Disconnected, message);
            }
        }

        /// <summary>
        /// Raises the StateUpdated event.
        /// </summary>
        /// <param name="state">The updated player state.</param>
        protected virtual void OnStateUpdated(PlayerState state)
        {
            StateUpdated?.Invoke(this, state);
        }

        /// <summary>
        /// Raises the ConnectionStatusChanged event.
        /// </summary>
        /// <param name="status">The new connection status.</param>
        /// <param name="message">The status message.</param>
        protected virtual void OnConnectionStatusChanged(ConnectionStatus status, string message)
        {
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(status, message));
        }
    }

    /// <summary>
    /// Event arguments for connection status change events.
    /// </summary>
    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the new connection status.
        /// </summary>
        public ConnectionStatus Status { get; }

        /// <summary>
        /// Gets the status message describing the connection state.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Initializes a new instance of the ConnectionStatusChangedEventArgs class.
        /// </summary>
        /// <param name="status">The connection status.</param>
        /// <param name="message">The status message.</param>
        public ConnectionStatusChangedEventArgs(ConnectionStatus status, string message)
        {
            Status = status;
            Message = message ?? string.Empty;
        }
    }
}
