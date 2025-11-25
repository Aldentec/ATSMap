using System;

namespace ProHauler.Core.Models
{
    /// <summary>
    /// Represents a detailed breakdown of a performance metric component.
    /// </summary>
    public class ScoreComponent
    {
        /// <summary>
        /// Gets or sets the name of the metric component.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the icon/emoji for the metric.
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current value of the component.
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Gets or sets the contribution percentage to the overall score (0-100).
        /// </summary>
        public float ContributionPercent { get; set; }

        /// <summary>
        /// Gets or sets the description of the component.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the color indicator (green for positive, red for negative, gray for neutral).
        /// </summary>
        public string ColorIndicator { get; set; } = "Gray";
    }

    /// <summary>
    /// Represents the complete breakdown of the overall performance score.
    /// </summary>
    public class ScoreBreakdown
    {
        /// <summary>
        /// Gets or sets the smoothness component breakdown.
        /// </summary>
        public ScoreComponent Smoothness { get; set; } = new ScoreComponent();

        /// <summary>
        /// Gets or sets the speed compliance component breakdown.
        /// </summary>
        public ScoreComponent SpeedCompliance { get; set; } = new ScoreComponent();

        /// <summary>
        /// Gets or sets the safety component breakdown.
        /// </summary>
        public ScoreComponent Safety { get; set; } = new ScoreComponent();

        /// <summary>
        /// Gets or sets the damage-free streak component breakdown.
        /// </summary>
        public ScoreComponent DamageFree { get; set; } = new ScoreComponent();
    }
}
