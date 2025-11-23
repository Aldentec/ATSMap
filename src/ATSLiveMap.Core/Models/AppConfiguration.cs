namespace ATSLiveMap.Core.Models
{
    public class AppConfiguration
    {
        // Properties with default values (like default parameters in JS)
        public string MapImagePath { get; set; } = "assets/maps/ats-map.png";
        public int TelemetryPollingIntervalMs { get; set; } = 50;
        public int ReconnectIntervalMs { get; set; } = 2000;
        public float MinZoom { get; set; } = 0.25f;  // f suffix means float literal
        public float MaxZoom { get; set; } = 4.0f;
        public bool EnableDiagnostics { get; set; } = false;
        public string DiagnosticsLogPath { get; set; } = "logs/telemetry.log";

        // Smoothing parameters
        public float PositionSmoothingFactor { get; set; } = 0.3f;
        public float HeadingSmoothingFactor { get; set; } = 0.5f;
    }
}
