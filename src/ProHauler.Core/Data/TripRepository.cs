using ProHauler.Core.Interfaces;
using ProHauler.Core.Models;
using Microsoft.Data.Sqlite;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace ProHauler.Core.Data
{
    /// <summary>
    /// Implements trip data access operations using SQLite database.
    /// Provides CRUD operations and aggregate statistics calculation for trip records.
    /// </summary>
    public class TripRepository : ITripRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the TripRepository class.
        /// </summary>
        /// <param name="databasePath">The file path to the SQLite database.</param>
        public TripRepository(string databasePath)
        {
            if (string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("Database path cannot be null or empty.", nameof(databasePath));
            }

            _connectionString = $"Data Source={databasePath}";
            Log.Information("TripRepository initialized with database: {DatabasePath}", databasePath);
        }

        /// <inheritdoc/>
        public async Task<int> SaveTripAsync(Trip trip)
        {
            if (trip == null)
            {
                throw new ArgumentNullException(nameof(trip));
            }

            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Trips (
                        StartTime, 
                        EndTime, 
                        DurationMinutes, 
                        DistanceMiles, 
                        SmoothnessScore, 
                        FuelEfficiencyMPG, 
                        SpeedCompliancePercent, 
                        SafetyScore,
                        OverallGrade, 
                        AverageSpeed, 
                        FuelConsumed
                    ) VALUES (
                        @StartTime, 
                        @EndTime, 
                        @DurationMinutes, 
                        @DistanceMiles, 
                        @SmoothnessScore, 
                        @FuelEfficiencyMPG, 
                        @SpeedCompliancePercent, 
                        @SafetyScore,
                        @OverallGrade, 
                        @AverageSpeed, 
                        @FuelConsumed
                    );
                    SELECT last_insert_rowid();";

                // Use ISO 8601 format for datetime storage
                command.Parameters.AddWithValue("@StartTime", trip.StartTime.ToString("o", CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@EndTime", trip.EndTime.ToString("o", CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@DurationMinutes", trip.DurationMinutes);
                command.Parameters.AddWithValue("@DistanceMiles", trip.DistanceMiles);
                command.Parameters.AddWithValue("@SmoothnessScore", trip.SmoothnessScore);
                command.Parameters.AddWithValue("@FuelEfficiencyMPG", trip.FuelEfficiencyMPG);
                command.Parameters.AddWithValue("@SpeedCompliancePercent", trip.SpeedCompliancePercent);
                command.Parameters.AddWithValue("@SafetyScore", trip.SafetyScore);
                command.Parameters.AddWithValue("@OverallGrade", trip.OverallGrade ?? string.Empty);
                command.Parameters.AddWithValue("@AverageSpeed", trip.AverageSpeed);
                command.Parameters.AddWithValue("@FuelConsumed", trip.FuelConsumed);

                var result = await command.ExecuteScalarAsync();
                var tripId = Convert.ToInt32(result);
                trip.Id = tripId;

                Log.Information("Trip saved successfully with ID: {TripId}, Grade: {Grade}, Distance: {Distance:F2} miles",
                    tripId, trip.OverallGrade, trip.DistanceMiles);

                return tripId;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Database error while saving trip");
                throw new InvalidOperationException("Failed to save trip to database. See inner exception for details.", ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while saving trip");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<Trip>> GetRecentTripsAsync(int count = 20)
        {
            if (count <= 0)
            {
                throw new ArgumentException("Count must be greater than zero.", nameof(count));
            }

            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        Id, 
                        StartTime, 
                        EndTime, 
                        DurationMinutes, 
                        DistanceMiles, 
                        SmoothnessScore, 
                        FuelEfficiencyMPG, 
                        SpeedCompliancePercent, 
                        SafetyScore,
                        OverallGrade, 
                        AverageSpeed, 
                        FuelConsumed
                    FROM Trips
                    ORDER BY StartTime DESC
                    LIMIT @Count";

                command.Parameters.AddWithValue("@Count", count);

                var trips = new List<Trip>();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    trips.Add(MapReaderToTrip(reader));
                }

                Log.Debug("Retrieved {Count} recent trips from database", trips.Count);
                return trips;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Database error while retrieving recent trips");
                throw new InvalidOperationException("Failed to retrieve recent trips from database. See inner exception for details.", ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while retrieving recent trips");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<Trip>> GetTripsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
            {
                throw new ArgumentException("End date must be greater than or equal to start date.");
            }

            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        Id, 
                        StartTime, 
                        EndTime, 
                        DurationMinutes, 
                        DistanceMiles, 
                        SmoothnessScore, 
                        FuelEfficiencyMPG, 
                        SpeedCompliancePercent, 
                        SafetyScore,
                        OverallGrade, 
                        AverageSpeed, 
                        FuelConsumed
                    FROM Trips
                    WHERE StartTime >= @StartDate AND StartTime <= @EndDate
                    ORDER BY StartTime DESC";

                // Use ISO 8601 format for date comparison
                command.Parameters.AddWithValue("@StartDate", startDate.ToString("o", CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@EndDate", endDate.ToString("o", CultureInfo.InvariantCulture));

                var trips = new List<Trip>();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    trips.Add(MapReaderToTrip(reader));
                }

                Log.Debug("Retrieved {Count} trips between {StartDate} and {EndDate}",
                    trips.Count, startDate.ToShortDateString(), endDate.ToShortDateString());
                return trips;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Database error while retrieving trips by date range");
                throw new InvalidOperationException("Failed to retrieve trips by date range from database. See inner exception for details.", ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while retrieving trips by date range");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<TripStatistics> GetStatisticsAsync()
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        COUNT(*) as TotalTrips,
                        COALESCE(SUM(DistanceMiles), 0) as TotalDistance,
                        COALESCE(SUM(DurationMinutes), 0) as TotalDuration,
                        COALESCE(AVG(SmoothnessScore), 0) as AvgSmoothness,
                        COALESCE(AVG(FuelEfficiencyMPG), 0) as AvgFuelEfficiency,
                        COALESCE(AVG(SpeedCompliancePercent), 0) as AvgSpeedCompliance,
                        COALESCE(AVG(SafetyScore), 0) as AvgSafety,
                        COALESCE(SUM(FuelConsumed), 0) as TotalFuel,
                        COALESCE(AVG(DistanceMiles), 0) as AvgTripDistance,
                        COALESCE(AVG(DurationMinutes), 0) as AvgTripDuration
                    FROM Trips";

                var statistics = new TripStatistics();

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    statistics.TotalTrips = reader.GetInt32(0);
                    statistics.TotalDistanceMiles = reader.GetFloat(1);
                    statistics.TotalDurationMinutes = reader.GetFloat(2);
                    statistics.AverageSmoothnessScore = reader.GetFloat(3);
                    statistics.AverageFuelEfficiencyMPG = reader.GetFloat(4);
                    statistics.AverageSpeedCompliancePercent = reader.GetFloat(5);
                    statistics.AverageSafetyScore = reader.GetFloat(6);
                    statistics.TotalFuelConsumed = reader.GetFloat(7);
                    statistics.AverageTripDistance = reader.GetFloat(8);
                    statistics.AverageTripDuration = reader.GetFloat(9);
                }

                // Calculate average overall score from component scores
                if (statistics.TotalTrips > 0)
                {
                    statistics.AverageOverallScore = (
                        statistics.AverageSmoothnessScore +
                        statistics.AverageSpeedCompliancePercent +
                        statistics.AverageSafetyScore
                    ) / 3.0f;
                }

                // Get best and worst grades
                var gradeCommand = connection.CreateCommand();
                gradeCommand.CommandText = @"
                    SELECT OverallGrade 
                    FROM Trips 
                    ORDER BY 
                        CASE OverallGrade
                            WHEN 'A+' THEN 1
                            WHEN 'A' THEN 2
                            WHEN 'B+' THEN 3
                            WHEN 'B' THEN 4
                            WHEN 'C+' THEN 5
                            WHEN 'C' THEN 6
                            WHEN 'D+' THEN 7
                            WHEN 'D' THEN 8
                            WHEN 'F' THEN 9
                            ELSE 10
                        END
                    LIMIT 1";

                var bestGrade = await gradeCommand.ExecuteScalarAsync();
                statistics.BestGrade = bestGrade?.ToString() ?? string.Empty;

                gradeCommand.CommandText = @"
                    SELECT OverallGrade 
                    FROM Trips 
                    ORDER BY 
                        CASE OverallGrade
                            WHEN 'A+' THEN 1
                            WHEN 'A' THEN 2
                            WHEN 'B+' THEN 3
                            WHEN 'B' THEN 4
                            WHEN 'C+' THEN 5
                            WHEN 'C' THEN 6
                            WHEN 'D+' THEN 7
                            WHEN 'D' THEN 8
                            WHEN 'F' THEN 9
                            ELSE 10
                        END DESC
                    LIMIT 1";

                var worstGrade = await gradeCommand.ExecuteScalarAsync();
                statistics.WorstGrade = worstGrade?.ToString() ?? string.Empty;

                Log.Debug("Retrieved statistics: {TotalTrips} trips, {AvgGrade:F1} average score",
                    statistics.TotalTrips, statistics.AverageOverallScore);

                return statistics;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Database error while calculating statistics");
                throw new InvalidOperationException("Failed to calculate trip statistics from database. See inner exception for details.", ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while calculating statistics");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<Trip>> GetAllTripsAsync()
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        Id, 
                        StartTime, 
                        EndTime, 
                        DurationMinutes, 
                        DistanceMiles, 
                        SmoothnessScore, 
                        FuelEfficiencyMPG, 
                        SpeedCompliancePercent, 
                        SafetyScore,
                        OverallGrade, 
                        AverageSpeed, 
                        FuelConsumed
                    FROM Trips
                    ORDER BY StartTime DESC";

                var trips = new List<Trip>();
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    trips.Add(MapReaderToTrip(reader));
                }

                Log.Debug("Retrieved {Count} total trips from database", trips.Count);
                return trips;
            }
            catch (SqliteException ex)
            {
                Log.Error(ex, "Database error while retrieving all trips");
                throw new InvalidOperationException("Failed to retrieve all trips from database. See inner exception for details.", ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error while retrieving all trips");
                throw;
            }
        }

        /// <summary>
        /// Maps a SqliteDataReader row to a Trip object.
        /// </summary>
        /// <param name="reader">The data reader positioned at a valid row.</param>
        /// <returns>A Trip object populated with data from the current row.</returns>
        private static Trip MapReaderToTrip(SqliteDataReader reader)
        {
            return new Trip
            {
                Id = reader.GetInt32(0),
                StartTime = DateTime.Parse(reader.GetString(1), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                EndTime = DateTime.Parse(reader.GetString(2), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                DurationMinutes = reader.GetFloat(3),
                DistanceMiles = reader.GetFloat(4),
                SmoothnessScore = reader.GetFloat(5),
                FuelEfficiencyMPG = reader.GetFloat(6),
                SpeedCompliancePercent = reader.GetFloat(7),
                SafetyScore = reader.GetFloat(8),
                OverallGrade = reader.GetString(9),
                AverageSpeed = reader.GetFloat(10),
                FuelConsumed = reader.GetFloat(11)
            };
        }
    }
}
