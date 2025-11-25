using Microsoft.Data.Sqlite;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ProHauler.Core.Data
{
    /// <summary>
    /// Handles database initialization and schema creation for the trip tracking system.
    /// </summary>
    public static class DatabaseInitializer
    {
        /// <summary>
        /// Initializes the SQLite database, creating the schema if it doesn't exist.
        /// </summary>
        /// <param name="databasePath">The file path to the SQLite database.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task InitializeDatabaseAsync(string databasePath)
        {
            try
            {
                // Ensure the directory exists
                var directory = Path.GetDirectoryName(databasePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Log.Information("Created database directory: {Directory}", directory);
                }

                // Create connection string
                var connectionString = $"Data Source={databasePath}";

                // Open connection and create schema
                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                // Create Trips table
                var createTableCommand = connection.CreateCommand();
                createTableCommand.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Trips (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        StartTime TEXT NOT NULL,
                        EndTime TEXT NOT NULL,
                        DurationMinutes REAL NOT NULL,
                        DistanceMiles REAL NOT NULL,
                        SmoothnessScore REAL NOT NULL,
                        FuelEfficiencyMPG REAL NOT NULL,
                        SpeedCompliancePercent REAL NOT NULL,
                        SafetyScore REAL NOT NULL DEFAULT 100.0,
                        OverallGrade TEXT NOT NULL,
                        AverageSpeed REAL NOT NULL,
                        FuelConsumed REAL NOT NULL
                    )";
                await createTableCommand.ExecuteNonQueryAsync();

                // Create index on StartTime
                var createIndexCommand = connection.CreateCommand();
                createIndexCommand.CommandText = @"
                    CREATE INDEX IF NOT EXISTS idx_trips_starttime 
                    ON Trips(StartTime DESC)";
                await createIndexCommand.ExecuteNonQueryAsync();

                // Create index on OverallGrade
                var createGradeIndexCommand = connection.CreateCommand();
                createGradeIndexCommand.CommandText = @"
                    CREATE INDEX IF NOT EXISTS idx_trips_grade 
                    ON Trips(OverallGrade)";
                await createGradeIndexCommand.ExecuteNonQueryAsync();

                Log.Information("Database initialized successfully at {DatabasePath}", databasePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize database at {DatabasePath}", databasePath);
                throw;
            }
        }

        /// <summary>
        /// Verifies that the database exists and is accessible.
        /// </summary>
        /// <param name="databasePath">The file path to the SQLite database.</param>
        /// <returns>True if the database is accessible, false otherwise.</returns>
        public static async Task<bool> VerifyDatabaseAsync(string databasePath)
        {
            try
            {
                if (!File.Exists(databasePath))
                {
                    return false;
                }

                var connectionString = $"Data Source={databasePath}";
                using var connection = new SqliteConnection(connectionString);
                await connection.OpenAsync();

                // Try to query the Trips table
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Trips";
                await command.ExecuteScalarAsync();

                return true;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Database verification failed for {DatabasePath}", databasePath);
                return false;
            }
        }
    }
}
