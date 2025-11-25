using ProHauler.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProHauler.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for trip data access operations.
    /// Provides methods for persisting and retrieving trip records from the database.
    /// </summary>
    public interface ITripRepository
    {
        /// <summary>
        /// Saves a trip record to the database asynchronously.
        /// </summary>
        /// <param name="trip">The trip to save. The Id property will be populated with the generated database ID.</param>
        /// <returns>The ID of the newly inserted trip record.</returns>
        Task<int> SaveTripAsync(Trip trip);

        /// <summary>
        /// Retrieves the most recent trips from the database.
        /// </summary>
        /// <param name="count">The maximum number of trips to retrieve. Defaults to 20.</param>
        /// <returns>A list of trips ordered by start time descending (newest first).</returns>
        Task<List<Trip>> GetRecentTripsAsync(int count = 20);

        /// <summary>
        /// Retrieves trips within a specific date range.
        /// </summary>
        /// <param name="startDate">The start of the date range (inclusive).</param>
        /// <param name="endDate">The end of the date range (inclusive).</param>
        /// <returns>A list of trips that started within the specified date range, ordered by start time descending.</returns>
        Task<List<Trip>> GetTripsByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Calculates aggregate statistics from all trips in the database.
        /// </summary>
        /// <returns>A TripStatistics object containing calculated aggregate values.</returns>
        Task<TripStatistics> GetStatisticsAsync();

        /// <summary>
        /// Retrieves all trips from the database.
        /// </summary>
        /// <returns>A list of all trips ordered by start time descending (newest first).</returns>
        Task<List<Trip>> GetAllTripsAsync();
    }
}
