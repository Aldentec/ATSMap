using System.Windows;

namespace ProHauler.Core.Models
{
    /// <summary>
    /// Represents a calibration point used for mapping game world coordinates to map pixel positions.
    /// Used in coordinate projection and map rendering systems.
    /// </summary>
    public class CalibrationPoint
    {
        /// <summary>
        /// Gets or sets the name of the location (e.g., city name or landmark).
        /// </summary>
        public string LocationName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the position in game world coordinates (ATS coordinate system).
        /// </summary>
        public Vector3 WorldPosition { get; set; }

        /// <summary>
        /// Gets or sets the corresponding pixel position on the map image.
        /// </summary>
        public Point MapPixelPosition { get; set; }
    }
}
