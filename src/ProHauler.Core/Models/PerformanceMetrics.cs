using System;
using System.Collections.Generic;

namespace ProHauler.Core.Models
{
    /// <summary>
    /// Represents real-time driving performance metrics for the current session.
    /// </summary>
    public class PerformanceMetrics
    {
        /// <summary>
        /// Gets or sets the smoothness score as a percentage (0-100%).
        /// Measures driving smoothness based on acceleration and braking patterns.
        /// Higher scores indicate smoother driving with gradual speed changes.
        /// </summary>
        public float SmoothnessScore { get; set; }

        /// <summary>
        /// Gets or sets the current fuel efficiency in miles per gallon (MPG).
        /// Calculated from distance traveled divided by fuel consumed.
        /// </summary>
        public float FuelEfficiencyMPG { get; set; }

        /// <summary>
        /// Gets or sets the fuel efficiency score as a percentage of the 6 MPG baseline.
        /// Values above 100% indicate better than baseline efficiency.
        /// </summary>
        public float FuelEfficiencyScore { get; set; }

        /// <summary>
        /// Gets or sets the speed compliance percentage (0-100%).
        /// Represents the percentage of time spent at or below the speed limit.
        /// </summary>
        public float SpeedCompliancePercent { get; set; }

        /// <summary>
        /// Gets or sets the speed compliance letter grade (A+ to F).
        /// Derived from the speed compliance percentage using standard grading scale.
        /// </summary>
        public string SpeedComplianceGrade { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the safety score (0-100%).
        /// Tracks proper use of signals, lights, brakes, and other safety practices.
        /// </summary>
        public float SafetyScore { get; set; }

        /// <summary>
        /// Gets or sets the damage-free streak duration.
        /// Tracks continuous driving time without vehicle damage.
        /// Resets to zero when damage increases.
        /// </summary>
        public TimeSpan DamageFreeStreak { get; set; }

        /// <summary>
        /// Gets or sets the damage-free score as a percentage (0-100%).
        /// Calculated from the damage-free streak duration using a logarithmic scale.
        /// </summary>
        public float DamageFreeScore { get; set; }

        /// <summary>
        /// Gets or sets the overall performance letter grade (A+ to F).
        /// Calculated by averaging smoothness, fuel efficiency, and speed compliance scores.
        /// </summary>
        public string OverallGrade { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the overall performance score as a percentage (0-100%).
        /// Average of smoothness, fuel efficiency, and speed compliance scores.
        /// </summary>
        public float OverallScore { get; set; }

        /// <summary>
        /// Gets or sets the performance trend indicator.
        /// Compares current performance to session average to show improvement or decline.
        /// </summary>
        public TrendIndicator Trend { get; set; }

        /// <summary>
        /// Gets or sets the session duration in minutes.
        /// Total time elapsed since session start.
        /// </summary>
        public float SessionDurationMinutes { get; set; }

        /// <summary>
        /// Gets or sets the session distance in miles.
        /// Total distance traveled since session start.
        /// </summary>
        public float SessionDistanceMiles { get; set; }

        /// <summary>
        /// Gets or sets the session average speed in miles per hour.
        /// Calculated from total distance divided by total time.
        /// </summary>
        public float SessionAverageSpeed { get; set; }

        /// <summary>
        /// Gets or sets the session fuel consumed in gallons.
        /// Total fuel used since session start.
        /// </summary>
        public float SessionFuelConsumed { get; set; }

        /// <summary>
        /// Gets or sets the historical smoothness score data for the last 60 seconds.
        /// Used for sparkline visualization.
        /// </summary>
        public List<float> SmoothnessHistory { get; set; } = new List<float>();

        /// <summary>
        /// Gets or sets the historical speed compliance data for the last 60 seconds.
        /// Used for sparkline visualization.
        /// </summary>
        public List<float> SpeedComplianceHistory { get; set; } = new List<float>();

        /// <summary>
        /// Gets or sets the historical safety score data for the last 60 seconds.
        /// Used for sparkline visualization.
        /// </summary>
        public List<float> SafetyHistory { get; set; } = new List<float>();

        /// <summary>
        /// Gets or sets the historical overall score data for the last 60 seconds.
        /// Used for sparkline visualization.
        /// </summary>
        public List<float> OverallHistory { get; set; } = new List<float>();
    }
}
