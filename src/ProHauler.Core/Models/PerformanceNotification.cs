using System;

namespace ProHauler.Core.Models
{
    /// <summary>
    /// Represents a notification about a performance event (penalty or reward).
    /// </summary>
    public class PerformanceNotification
    {
        /// <summary>
        /// Gets or sets the message describing the event.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the point change (negative for penalties, positive for rewards).
        /// </summary>
        public float PointChange { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the notification was created.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the type of notification (Penalty, Reward, or Neutral).
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Gets or sets the category of the notification (Smoothness, Safety, Speed, etc.).
        /// </summary>
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Defines the type of performance notification.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// A penalty that reduces the score.
        /// </summary>
        Penalty,

        /// <summary>
        /// A reward that increases the score.
        /// </summary>
        Reward,

        /// <summary>
        /// A neutral informational notification.
        /// </summary>
        Neutral
    }
}
