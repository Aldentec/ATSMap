using System;

namespace ATSLiveMap.Core.Models
{
    // class is like a class in JS/TS
    public class TelemetryData
    {
        public bool IsConnected { get; set; }
        public string GameName { get; set; } = string.Empty;  // = "" in JS
        public bool IsPaused { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }      // In radians
        public float Pitch { get; set; }
        public float Roll { get; set; }
        public float Speed { get; set; }        // In m/s
        public DateTime Timestamp { get; set; }  // Like Date in JS
    }
}
