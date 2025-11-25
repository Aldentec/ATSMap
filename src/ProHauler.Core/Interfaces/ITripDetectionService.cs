using ProHauler.Core.Models;
using System.Threading.Tasks;

namespace ProHauler.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for automatic trip detection and lifecycle management.
    /// Monitors telemetry data to detect trip start/end and manages trip persistence.
    /// </summary>
    public interface ITripDetectionService
    {
        /// <summary>
        /// Gets a value indicating whether a trip is currently active.
        /// </summary>
        bool IsTripActive { get; }

        /// <summary>
        /// Gets the current active trip, or null if no trip is in progress.
        /// </summary>
        Trip? CurrentTrip { get; }

        /// <summary>
        /// Updates trip tracking based on new telemetry data.
        /// Handles trip start detection, distance/fuel accumulation, and trip end detection.
        /// </summary>
        /// <param name="data">The latest telemetry data from the game.</param>
        void UpdateFromTelemetry(TelemetryData data);

        /// <summary>
        /// Ends the current trip and saves it to the database.
        /// Called when trip end is detected or application is closing.
        /// </summary>
        /// <returns>The completed trip, or null if no trip was active.</returns>
        Task<Trip?> EndCurrentTripAsync();
    }
}
