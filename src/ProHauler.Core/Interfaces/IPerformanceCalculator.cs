using System;
using ProHauler.Core.Models;

namespace ProHauler.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for calculating real-time driving performance metrics.
    /// </summary>
    public interface IPerformanceCalculator
    {
        /// <summary>
        /// Event raised when a performance notification (penalty or reward) occurs.
        /// </summary>
        event EventHandler<PerformanceNotification>? NotificationRaised;

        /// <summary>
        /// Gets the current performance metrics for the active session.
        /// </summary>
        /// <returns>A PerformanceMetrics object containing all current scores and statistics.</returns>
        PerformanceMetrics GetCurrentMetrics();

        /// <summary>
        /// Gets the current score breakdown showing how each component contributes to the overall score.
        /// </summary>
        /// <returns>A ScoreBreakdown object containing detailed component information.</returns>
        ScoreBreakdown GetScoreBreakdown();

        /// <summary>
        /// Updates performance calculations based on new telemetry data.
        /// </summary>
        /// <param name="data">The latest telemetry data from the game.</param>
        void UpdateFromTelemetry(TelemetryData data);

        /// <summary>
        /// Resets all session statistics and scores to initial values.
        /// </summary>
        void ResetSession();

        /// <summary>
        /// Gets detailed tooltip information for the smoothness metric.
        /// </summary>
        /// <returns>A MetricTooltipInfo object with calculation details and tips.</returns>
        MetricTooltipInfo GetSmoothnessTooltip();

        /// <summary>
        /// Gets detailed tooltip information for the speed compliance metric.
        /// </summary>
        /// <returns>A MetricTooltipInfo object with calculation details and tips.</returns>
        MetricTooltipInfo GetSpeedComplianceTooltip();

        /// <summary>
        /// Gets detailed tooltip information for the safety metric.
        /// </summary>
        /// <returns>A MetricTooltipInfo object with calculation details and tips.</returns>
        MetricTooltipInfo GetSafetyTooltip();

        /// <summary>
        /// Gets detailed tooltip information for the overall score metric.
        /// </summary>
        /// <returns>A MetricTooltipInfo object with calculation details and tips.</returns>
        MetricTooltipInfo GetOverallTooltip();
    }
}
