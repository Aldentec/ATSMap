using System;
using ProHauler.Core.Models;

namespace ProHauler.Core.Interfaces
{
    /// <summary>
    /// Represents the connection status to the game telemetry system.
    /// </summary>
    public enum ConnectionStatus
    {
        /// <summary>
        /// Not connected to the game.
        /// </summary>
        Disconnected,

        /// <summary>
        /// Attempting to establish connection.
        /// </summary>
        Connecting,

        /// <summary>
        /// Successfully connected and receiving data.
        /// </summary>
        Connected,

        /// <summary>
        /// Connection error occurred.
        /// </summary>
        Error
    }

    /// <summary>
    /// Defines the contract for managing player state from telemetry data.
    /// Handles coordinate projection, smoothing, and state change notifications.
    /// </summary>
    public interface IStateManager
    {
        /// <summary>
        /// Gets the current player state with both raw and smoothed values.
        /// </summary>
        PlayerState CurrentState { get; }

        /// <summary>
        /// Gets the current connection status.
        /// </summary>
        ConnectionStatus Status { get; }

        /// <summary>
        /// Event raised when player state is updated with new telemetry data.
        /// </summary>
        event EventHandler<PlayerState> StateUpdated;

        /// <summary>
        /// Updates the player state from new telemetry data.
        /// Applies coordinate projection and smoothing.
        /// </summary>
        /// <param name="data">The latest telemetry data from the game.</param>
        void UpdateFromTelemetry(TelemetryData data);
    }
}
