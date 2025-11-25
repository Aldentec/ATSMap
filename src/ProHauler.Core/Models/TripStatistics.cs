namespace ProHauler.Core.Models
{
    /// <summary>
    /// Represents aggregate statistics calculated from historical trip data.
    /// Used for displaying summary information and comparing current performance to averages.
    /// </summary>
    public class TripStatistics
    {
        /// <summary>
        /// Gets or sets the total number of trips recorded in the database.
        /// </summary>
        public int TotalTrips { get; set; }

        /// <summary>
        /// Gets or sets the total distance traveled across all trips in miles.
        /// </summary>
        public float TotalDistanceMiles { get; set; }

        /// <summary>
        /// Gets or sets the total driving time across all trips in minutes.
        /// </summary>
        public float TotalDurationMinutes { get; set; }

        /// <summary>
        /// Gets or sets the average smoothness score across all trips (0-100%).
        /// </summary>
        public float AverageSmoothnessScore { get; set; }

        /// <summary>
        /// Gets or sets the average fuel efficiency across all trips in MPG.
        /// </summary>
        public float AverageFuelEfficiencyMPG { get; set; }

        /// <summary>
        /// Gets or sets the average speed compliance percentage across all trips (0-100%).
        /// </summary>
        public float AverageSpeedCompliancePercent { get; set; }

        /// <summary>
        /// Gets or sets the average safety score across all trips (0-100%).
        /// </summary>
        public float AverageSafetyScore { get; set; }

        /// <summary>
        /// Gets or sets the average overall score across all trips (0-100%).
        /// Calculated from the average of all performance metrics.
        /// </summary>
        public float AverageOverallScore { get; set; }

        /// <summary>
        /// Gets or sets the best overall grade achieved across all trips.
        /// </summary>
        public string BestGrade { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the worst overall grade achieved across all trips.
        /// </summary>
        public string WorstGrade { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total fuel consumed across all trips in gallons.
        /// </summary>
        public float TotalFuelConsumed { get; set; }

        /// <summary>
        /// Gets or sets the average trip distance in miles.
        /// </summary>
        public float AverageTripDistance { get; set; }

        /// <summary>
        /// Gets or sets the average trip duration in minutes.
        /// </summary>
        public float AverageTripDuration { get; set; }
    }
}
