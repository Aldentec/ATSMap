using System;
using System.Windows;
using ProHauler.Core.Interfaces;
using ProHauler.Core.Models;

namespace ProHauler.UI.Views
{
    public partial class DebugWindow : Window
    {
        private readonly ITelemetryClient _telemetryClient;
        private readonly IPerformanceCalculator _performanceCalculator;
        private System.Timers.Timer? _updateTimer;
        private float _previousHeading = 0;
        private DateTime _previousTime = DateTime.Now;

        public DebugWindow(ITelemetryClient telemetryClient, IPerformanceCalculator performanceCalculator)
        {
            InitializeComponent();
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _performanceCalculator = performanceCalculator ?? throw new ArgumentNullException(nameof(performanceCalculator));

            // Update debug output every 100ms
            _updateTimer = new System.Timers.Timer(100);
            _updateTimer.Elapsed += (s, e) => UpdateDebugOutput();
            _updateTimer.Start();
        }

        private void UpdateDebugOutput()
        {
            Dispatcher.Invoke(() =>
            {
                var data = _telemetryClient.GetCurrentData();
                var metrics = _performanceCalculator.GetCurrentMetrics();

                if (data == null)
                {
                    DebugOutput.Text = "No telemetry data available";
                    return;
                }

                // Calculate heading change rate
                DateTime now = DateTime.Now;
                float deltaTime = (float)(now - _previousTime).TotalSeconds;
                float headingDiff = data.Heading - _previousHeading;

                // Normalize heading difference
                while (headingDiff > Math.PI) headingDiff -= (float)(2 * Math.PI);
                while (headingDiff < -Math.PI) headingDiff += (float)(2 * Math.PI);

                float headingChangeDegPerSec = 0;
                if (deltaTime > 0)
                {
                    float headingChangePerSec = Math.Abs(headingDiff) / deltaTime;
                    headingChangeDegPerSec = headingChangePerSec * (180.0f / (float)Math.PI);
                }

                _previousHeading = data.Heading;
                _previousTime = now;

                var output = $@"=== TELEMETRY DATA ===
Connected: {data.IsConnected}
Paused: {data.IsPaused}
Game: {data.GameName}

Position: ({data.Position.X:F1}, {data.Position.Y:F1}, {data.Position.Z:F1})
Heading: {data.Heading:F3} rad ({data.Heading * 180 / Math.PI:F1}°)
Heading Change Rate: {headingChangeDegPerSec:F1}°/sec
Pitch: {data.Pitch:F3} rad
Roll: {data.Roll:F3} rad

Speed: {data.Speed:F2} km/h ({data.Speed * 0.621371f:F1} MPH)
Speed Limit: {data.SpeedLimit:F2} km/h

Fuel: {data.FuelAmount:F2} / {data.FuelCapacity:F2} L
Damage: {data.DamagePercent:F2}%
Odometer: {data.Odometer:F2} km ({data.Odometer * 0.621371f:F2} mi)

=== PERFORMANCE METRICS ===
Smoothness: {metrics.SmoothnessScore:F1}%
Speed Compliance: {metrics.SpeedCompliancePercent:F1}% (Grade: {metrics.SpeedComplianceGrade})
Damage-Free Streak: {metrics.DamageFreeStreak.Hours:D2}:{metrics.DamageFreeStreak.Minutes:D2}:{metrics.DamageFreeStreak.Seconds:D2}

Overall Score: {metrics.OverallScore:F1}%
Overall Grade: {metrics.OverallGrade}
Trend: {metrics.Trend}

Session Duration: {metrics.SessionDurationMinutes:F2} min
Session Distance: {metrics.SessionDistanceMiles:F2} mi
Session Avg Speed: {metrics.SessionAverageSpeed:F1} MPH

Last Update: {DateTime.Now:HH:mm:ss.fff}";

                DebugOutput.Text = output;
            });
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _updateTimer?.Stop();
            _updateTimer?.Dispose();
            base.OnClosing(e);
        }
    }
}
