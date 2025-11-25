namespace ProHauler.Core.Models
{
    /// <summary>
    /// Contains detailed information for metric tooltips.
    /// Provides educational transparency about how metrics are calculated.
    /// </summary>
    public class MetricTooltipInfo
    {
        /// <summary>
        /// Gets or sets the metric name.
        /// </summary>
        public string MetricName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the icon representing the metric.
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current score value.
        /// </summary>
        public float CurrentValue { get; set; }

        /// <summary>
        /// Gets or sets the explanation of how the metric is calculated.
        /// </summary>
        public string CalculationExplanation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current penalties affecting the score.
        /// </summary>
        public string CurrentPenalties { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current bonuses affecting the score.
        /// </summary>
        public string CurrentBonuses { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets tips for improving the metric.
        /// </summary>
        public string ImprovementTips { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the weight of this metric in the overall score.
        /// </summary>
        public float Weight { get; set; }
    }
}
