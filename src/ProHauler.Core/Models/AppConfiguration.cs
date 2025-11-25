namespace ProHauler.Core.Models;

/// <summary>
/// Application configuration settings
/// </summary>
public class AppConfiguration
{
    /// <summary>
    /// Path to the map image file
    /// </summary>
    public string MapImagePath { get; set; } = "assets/maps/ats-map.png";

    /// <summary>
    /// Telemetry polling interval in milliseconds
    /// </summary>
    public int TelemetryPollingIntervalMs { get; set; } = 50;

    /// <summary>
    /// Reconnection interval in milliseconds
    /// </summary>
    public int ReconnectIntervalMs { get; set; } = 2000;

    /// <summary>
    /// Minimum zoom level
    /// </summary>
    public double MinZoom { get; set; } = 0.25;

    /// <summary>
    /// Maximum zoom level
    /// </summary>
    public double MaxZoom { get; set; } = 4.0;

    /// <summary>
    /// Target frame rate for rendering
    /// </summary>
    public int TargetFrameRate { get; set; } = 30;

    /// <summary>
    /// Position smoothing factor (0 = no smoothing, 1 = instant)
    /// </summary>
    public float PositionSmoothingFactor { get; set; } = 0.3f;

    /// <summary>
    /// Heading smoothing factor (0 = no smoothing, 1 = instant)
    /// </summary>
    public float HeadingSmoothingFactor { get; set; } = 0.5f;

    /// <summary>
    /// Enable diagnostic logging
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Log file path
    /// </summary>
    public string LogPath { get; set; } = "logs/telemetry.log";

    /// <summary>
    /// Log level (Information, Warning, Error, etc.)
    /// </summary>
    public string LogLevel { get; set; } = "Information";
}
