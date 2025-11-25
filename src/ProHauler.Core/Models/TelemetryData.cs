using System;

namespace ProHauler.Core.Models
{
    /// <summary>
    /// Represents telemetry data received from the game (American Truck Simulator).
    /// Contains vehicle state, position, performance metrics, and safety-related information.
    /// </summary>
    public class TelemetryData
    {
        /// <summary>
        /// Gets or sets whether the telemetry connection is active.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// Gets or sets the name of the game providing telemetry data.
        /// </summary>
        public string GameName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the game is currently paused.
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>
        /// Gets or sets the vehicle's 3D position in game world coordinates.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the vehicle heading in radians.
        /// </summary>
        public float Heading { get; set; }

        /// <summary>
        /// Gets or sets the vehicle pitch angle in radians.
        /// </summary>
        public float Pitch { get; set; }

        /// <summary>
        /// Gets or sets the vehicle roll angle in radians.
        /// </summary>
        public float Roll { get; set; }

        /// <summary>
        /// Gets or sets the vehicle speed in meters per second.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when this telemetry data was captured.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the current fuel amount in liters.
        /// </summary>
        public float FuelAmount { get; set; }

        /// <summary>
        /// Gets or sets the fuel tank capacity in liters.
        /// </summary>
        public float FuelCapacity { get; set; }

        /// <summary>
        /// Gets or sets the total vehicle damage as a percentage (0-100%).
        /// </summary>
        public float DamagePercent { get; set; }

        /// <summary>
        /// Gets or sets the total distance traveled in kilometers.
        /// </summary>
        public float Odometer { get; set; }

        /// <summary>
        /// Gets or sets the current speed limit in the game (0 if unknown).
        /// </summary>
        public float SpeedLimit { get; set; }

        /// <summary>
        /// Gets or sets whether the left turn signal is active.
        /// </summary>
        public bool BlinkerLeftActive { get; set; }

        /// <summary>
        /// Gets or sets whether the right turn signal is active.
        /// </summary>
        public bool BlinkerRightActive { get; set; }

        /// <summary>
        /// Gets or sets whether the high beam headlights are on.
        /// </summary>
        public bool LightsBeamHighOn { get; set; }

        /// <summary>
        /// Gets or sets whether the parking brake is engaged.
        /// </summary>
        public bool ParkBrakeOn { get; set; }

        /// <summary>
        /// Gets or sets whether the engine brake (jake brake) is active.
        /// </summary>
        public bool MotorBrakeOn { get; set; }

        /// <summary>
        /// Gets or sets the retarder brake level (0 = off, higher values = more braking).
        /// </summary>
        public int RetarderBrake { get; set; }

        /// <summary>
        /// Gets or sets whether cruise control is enabled.
        /// </summary>
        public bool CruiseControlOn { get; set; }

        /// <summary>
        /// Gets or sets the current engine RPM (revolutions per minute).
        /// </summary>
        public float EngineRpm { get; set; }

        /// <summary>
        /// Gets or sets the maximum engine RPM.
        /// </summary>
        public float EngineRpmMax { get; set; }

        /// <summary>
        /// Gets or sets the brake temperature in degrees Celsius.
        /// </summary>
        public float BrakeTemperature { get; set; }
    }
}
