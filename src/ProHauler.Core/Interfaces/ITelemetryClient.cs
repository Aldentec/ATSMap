using System;
using System.Threading.Tasks;
using ProHauler.Core.Models;

namespace ProHauler.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for telemetry client implementations that connect to the game.
    /// Provides methods for retrieving telemetry data and managing the connection lifecycle.
    /// </summary>
    public interface ITelemetryClient
    {
        /// <summary>
        /// Gets a value indicating whether the telemetry client is currently connected to the game.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the current telemetry data synchronously.
        /// </summary>
        /// <returns>The latest telemetry data from the game.</returns>
        TelemetryData GetCurrentData();

        /// <summary>
        /// Gets the current telemetry data asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the latest telemetry data.</returns>
        Task<TelemetryData> GetCurrentDataAsync();

        /// <summary>
        /// Event raised when new telemetry data is received from the game.
        /// </summary>
        event EventHandler<TelemetryData> DataUpdated;

        /// <summary>
        /// Event raised when the connection status changes (connected or disconnected).
        /// </summary>
        event EventHandler<string> ConnectionStatusChanged;

        /// <summary>
        /// Starts the telemetry client and begins polling for data.
        /// </summary>
        /// <returns>A task that represents the asynchronous start operation.</returns>
        Task StartAsync();

        /// <summary>
        /// Stops the telemetry client and closes the connection.
        /// </summary>
        /// <returns>A task that represents the asynchronous stop operation.</returns>
        Task StopAsync();
    }
}
