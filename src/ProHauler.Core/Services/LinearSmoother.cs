using System;
using System.Windows;

namespace ProHauler.Core.Services
{
    /// <summary>
    /// Provides linear interpolation smoothing for position and heading values
    /// to reduce visual jitter in real-time updates.
    /// </summary>
    public class LinearSmoother
    {
        private readonly float _positionSmoothingFactor;
        private readonly float _headingSmoothingFactor;

        /// <summary>
        /// Initializes a new instance of the LinearSmoother class.
        /// </summary>
        /// <param name="positionSmoothingFactor">
        /// Smoothing factor for position (0.0 to 1.0). 
        /// Higher values = more responsive but less smooth. Default is 0.3.
        /// </param>
        /// <param name="headingSmoothingFactor">
        /// Smoothing factor for heading (0.0 to 1.0).
        /// Higher values = more responsive but less smooth. Default is 0.5.
        /// </param>
        public LinearSmoother(float positionSmoothingFactor = 0.3f, float headingSmoothingFactor = 0.5f)
        {
            // Clamp smoothing factors to valid range [0, 1]
            _positionSmoothingFactor = Math.Clamp(positionSmoothingFactor, 0.0f, 1.0f);
            _headingSmoothingFactor = Math.Clamp(headingSmoothingFactor, 0.0f, 1.0f);
        }

        /// <summary>
        /// Applies linear interpolation to smooth position values.
        /// Formula: smoothed = last + (new - last) * factor
        /// </summary>
        /// <param name="lastPosition">The previous smoothed position.</param>
        /// <param name="newPosition">The new raw position from telemetry.</param>
        /// <returns>The smoothed position.</returns>
        public Point SmoothPosition(Point lastPosition, Point newPosition)
        {
            // Linear interpolation for X coordinate
            double smoothedX = lastPosition.X + (newPosition.X - lastPosition.X) * _positionSmoothingFactor;

            // Linear interpolation for Y coordinate
            double smoothedY = lastPosition.Y + (newPosition.Y - lastPosition.Y) * _positionSmoothingFactor;

            return new Point(smoothedX, smoothedY);
        }

        /// <summary>
        /// Applies linear interpolation to smooth heading angle values.
        /// Handles angle wrapping to ensure shortest rotation path.
        /// </summary>
        /// <param name="lastHeading">The previous smoothed heading in radians.</param>
        /// <param name="newHeading">The new raw heading from telemetry in radians.</param>
        /// <returns>The smoothed heading in radians.</returns>
        public float SmoothHeading(float lastHeading, float newHeading)
        {
            // Calculate the difference between angles
            float delta = newHeading - lastHeading;

            // Normalize delta to [-π, π] to ensure shortest rotation path
            // This handles the wrap-around at 0/2π
            while (delta > Math.PI)
            {
                delta -= (float)(2 * Math.PI);
            }
            while (delta < -Math.PI)
            {
                delta += (float)(2 * Math.PI);
            }

            // Apply linear interpolation
            float smoothedHeading = lastHeading + delta * _headingSmoothingFactor;

            // Normalize result to [0, 2π]
            while (smoothedHeading < 0)
            {
                smoothedHeading += (float)(2 * Math.PI);
            }
            while (smoothedHeading >= 2 * Math.PI)
            {
                smoothedHeading -= (float)(2 * Math.PI);
            }

            return smoothedHeading;
        }

        /// <summary>
        /// Calculates the maximum lag introduced by smoothing.
        /// This helps ensure smoothing doesn't exceed the 500ms lag requirement.
        /// </summary>
        /// <param name="updateIntervalMs">The telemetry update interval in milliseconds.</param>
        /// <returns>Estimated maximum lag in milliseconds.</returns>
        public float CalculateMaxLag(int updateIntervalMs)
        {
            // With linear interpolation, the lag is approximately:
            // lag ≈ updateInterval / smoothingFactor
            // We use the position factor as it's typically lower (more smoothing)
            float estimatedLag = updateIntervalMs / _positionSmoothingFactor;
            return estimatedLag;
        }
    }
}
