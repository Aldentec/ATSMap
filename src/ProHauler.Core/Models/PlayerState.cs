using System;
using System.Windows;

namespace ProHauler.Core.Models
{
    /// <summary>
    /// Represents the player's vehicle state for map rendering and visualization.
    /// Includes both raw and smoothed values to reduce visual jitter.
    /// </summary>
    public class PlayerState
    {
        /// <summary>
        /// Gets or sets the vehicle's position on the map in 2D coordinates.
        /// </summary>
        public Point MapPosition { get; set; }

        /// <summary>
        /// Gets or sets the vehicle heading in radians.
        /// </summary>
        public float Heading { get; set; }

        /// <summary>
        /// Gets or sets the vehicle speed.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of this state snapshot.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the smoothed map position for rendering (reduces visual jitter).
        /// </summary>
        public Point SmoothedMapPosition { get; set; }

        /// <summary>
        /// Gets or sets the smoothed heading for rendering (reduces visual jitter).
        /// </summary>
        public float SmoothedHeading { get; set; }
    }
}
