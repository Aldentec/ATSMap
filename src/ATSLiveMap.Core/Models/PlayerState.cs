using System;
using System.Windows;  // For Point type (X, Y coordinates)

namespace ATSLiveMap.Core.Models
{
    public class PlayerState
    {
        // Point is a WPF type for 2D coordinates (like {x, y} object in JS)
        public Point MapPosition { get; set; }
        public float Heading { get; set; }
        public float Speed { get; set; }
        public DateTime Timestamp { get; set; }

        // Smoothed values for rendering (reduces jitter)
        public Point SmoothedMapPosition { get; set; }
        public float SmoothedHeading { get; set; }
    }
}
