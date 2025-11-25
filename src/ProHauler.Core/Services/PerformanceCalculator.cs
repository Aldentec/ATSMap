using System;
using System.Collections.Generic;
using System.Linq;
using ProHauler.Core.Helpers;
using ProHauler.Core.Interfaces;
using ProHauler.Core.Models;

namespace ProHauler.Core.Services
{
    /// <summary>
    /// Calculates real-time driving performance metrics from telemetry data.
    /// </summary>
    public class PerformanceCalculator : IPerformanceCalculator
    {
        private readonly object _lock = new object();
        private CalculationState _state;
        private PerformanceMetrics _currentMetrics;
        private ScoreBreakdown _currentBreakdown;

        /// <summary>
        /// Event raised when a performance notification (penalty or reward) occurs.
        /// </summary>
        public event EventHandler<PerformanceNotification>? NotificationRaised;

        public PerformanceCalculator()
        {
            _state = new CalculationState();
            _currentMetrics = new PerformanceMetrics();
            _currentBreakdown = new ScoreBreakdown();
            ResetSession();
        }

        /// <summary>
        /// Raises a notification event for a performance penalty or reward.
        /// </summary>
        private void RaiseNotification(string message, float pointChange, string category)
        {
            var notification = new PerformanceNotification
            {
                Message = message,
                PointChange = pointChange,
                Timestamp = DateTime.Now,
                Type = pointChange < 0 ? NotificationType.Penalty :
                       pointChange > 0 ? NotificationType.Reward :
                       NotificationType.Neutral,
                Category = category
            };

            NotificationRaised?.Invoke(this, notification);
        }

        /// <summary>
        /// Gets the current performance metrics for the active session.
        /// Returns a copy to ensure proper change notification in UI bindings.
        /// </summary>
        public PerformanceMetrics GetCurrentMetrics()
        {
            lock (_lock)
            {
                // Return a copy so WPF detects property changes
                return new PerformanceMetrics
                {
                    SmoothnessScore = _currentMetrics.SmoothnessScore,
                    FuelEfficiencyMPG = _currentMetrics.FuelEfficiencyMPG,
                    FuelEfficiencyScore = _currentMetrics.FuelEfficiencyScore,
                    SpeedCompliancePercent = _currentMetrics.SpeedCompliancePercent,
                    SpeedComplianceGrade = _currentMetrics.SpeedComplianceGrade,
                    SafetyScore = _currentMetrics.SafetyScore,
                    DamageFreeStreak = _currentMetrics.DamageFreeStreak,
                    DamageFreeScore = _currentMetrics.DamageFreeScore,
                    OverallGrade = _currentMetrics.OverallGrade,
                    OverallScore = _currentMetrics.OverallScore,
                    Trend = _currentMetrics.Trend,
                    SessionDurationMinutes = _currentMetrics.SessionDurationMinutes,
                    SessionDistanceMiles = _currentMetrics.SessionDistanceMiles,
                    SessionAverageSpeed = _currentMetrics.SessionAverageSpeed,
                    SessionFuelConsumed = _currentMetrics.SessionFuelConsumed,
                    SmoothnessHistory = new List<float>(_state.SmoothnessHistory),
                    SpeedComplianceHistory = new List<float>(_state.SpeedComplianceHistory),
                    SafetyHistory = new List<float>(_state.SafetyHistory),
                    OverallHistory = new List<float>(_state.OverallScoreHistory)
                };
            }
        }

        /// <summary>
        /// Gets the current score breakdown showing how each component contributes to the overall score.
        /// </summary>
        public ScoreBreakdown GetScoreBreakdown()
        {
            lock (_lock)
            {
                UpdateScoreBreakdown();

                // Return a copy
                return new ScoreBreakdown
                {
                    Smoothness = new ScoreComponent
                    {
                        Name = _currentBreakdown.Smoothness.Name,
                        Icon = _currentBreakdown.Smoothness.Icon,
                        Value = _currentBreakdown.Smoothness.Value,
                        ContributionPercent = _currentBreakdown.Smoothness.ContributionPercent,
                        Description = _currentBreakdown.Smoothness.Description,
                        ColorIndicator = _currentBreakdown.Smoothness.ColorIndicator
                    },
                    SpeedCompliance = new ScoreComponent
                    {
                        Name = _currentBreakdown.SpeedCompliance.Name,
                        Icon = _currentBreakdown.SpeedCompliance.Icon,
                        Value = _currentBreakdown.SpeedCompliance.Value,
                        ContributionPercent = _currentBreakdown.SpeedCompliance.ContributionPercent,
                        Description = _currentBreakdown.SpeedCompliance.Description,
                        ColorIndicator = _currentBreakdown.SpeedCompliance.ColorIndicator
                    },
                    Safety = new ScoreComponent
                    {
                        Name = _currentBreakdown.Safety.Name,
                        Icon = _currentBreakdown.Safety.Icon,
                        Value = _currentBreakdown.Safety.Value,
                        ContributionPercent = _currentBreakdown.Safety.ContributionPercent,
                        Description = _currentBreakdown.Safety.Description,
                        ColorIndicator = _currentBreakdown.Safety.ColorIndicator
                    },
                    DamageFree = new ScoreComponent
                    {
                        Name = _currentBreakdown.DamageFree.Name,
                        Icon = _currentBreakdown.DamageFree.Icon,
                        Value = _currentBreakdown.DamageFree.Value,
                        ContributionPercent = _currentBreakdown.DamageFree.ContributionPercent,
                        Description = _currentBreakdown.DamageFree.Description,
                        ColorIndicator = _currentBreakdown.DamageFree.ColorIndicator
                    }
                };
            }
        }

        /// <summary>
        /// Gets detailed tooltip information for the smoothness metric.
        /// </summary>
        public MetricTooltipInfo GetSmoothnessTooltip()
        {
            lock (_lock)
            {
                return new MetricTooltipInfo
                {
                    MetricName = "Smoothness Score",
                    Icon = "🎯",
                    CurrentValue = _currentMetrics.SmoothnessScore,
                    Weight = 25.0f,
                    CalculationExplanation = "Measures driving smoothness based on acceleration, braking, and steering patterns.\n\n" +
                        "• Starts at 100% and adjusts based on driving inputs\n" +
                        "• Harsh acceleration (>12 MPH/sec): -1.5 pts\n" +
                        "• Moderate acceleration (>8 MPH/sec): -0.4 pts\n" +
                        "• Harsh braking (>18 MPH/sec): -3.0 pts\n" +
                        "• Hard braking (>12 MPH/sec): -1.0 pts\n" +
                        "• Sharp swerving (>3.5°/sec): -1.5 pts\n" +
                        "• Smooth driving: +0.2-0.4 pts/sec",
                    CurrentPenalties = GetSmoothnessCurrentPenalties(),
                    CurrentBonuses = GetSmoothnessCurrentBonuses(),
                    ImprovementTips = "• Accelerate gradually and smoothly\n" +
                        "• Anticipate stops and brake early\n" +
                        "• Avoid sudden steering inputs\n" +
                        "• Maintain steady speed when possible\n" +
                        "• Use cruise control on highways"
                };
            }
        }

        /// <summary>
        /// Gets detailed tooltip information for the speed compliance metric.
        /// </summary>
        public MetricTooltipInfo GetSpeedComplianceTooltip()
        {
            lock (_lock)
            {
                float speedLimit = 65.0f; // Default, would need current telemetry for actual
                return new MetricTooltipInfo
                {
                    MetricName = "Speed Compliance",
                    Icon = "🚦",
                    CurrentValue = _currentMetrics.SpeedCompliancePercent,
                    Weight = 25.0f,
                    CalculationExplanation = "Tracks adherence to speed limits over time.\n\n" +
                        $"• Current speed limit: {speedLimit:F0} MPH\n" +
                        "• Calculates percentage of time at or below limit\n" +
                        "• Highway limit: 65 MPH\n" +
                        "• Default limit: 55 MPH\n" +
                        "• Uses posted limits when available",
                    CurrentPenalties = GetSpeedComplianceCurrentPenalties(),
                    CurrentBonuses = GetSpeedComplianceCurrentBonuses(),
                    ImprovementTips = "• Watch for speed limit signs\n" +
                        "• Use cruise control to maintain speed\n" +
                        "• Slow down in urban areas\n" +
                        "• Check speedometer regularly\n" +
                        "• Allow extra time for trips"
                };
            }
        }

        /// <summary>
        /// Gets detailed tooltip information for the safety metric.
        /// </summary>
        public MetricTooltipInfo GetSafetyTooltip()
        {
            lock (_lock)
            {
                return new MetricTooltipInfo
                {
                    MetricName = "Safety Score",
                    Icon = "🛡️",
                    CurrentValue = _currentMetrics.SafetyScore,
                    Weight = 25.0f,
                    CalculationExplanation = "Monitors proper use of safety equipment and practices.\n\n" +
                        "• Starts at 100% and adjusts based on behavior\n" +
                        "• Turn without blinker: -2.0 pts\n" +
                        "• Parking brake while moving: -5.0 pts/sec\n" +
                        "• Inappropriate high beams: -0.5 pts/sec\n" +
                        "• Over-revving engine (>90% RPM): -1.0 pts/sec\n" +
                        "• Hot brakes (>200°C): -1.0 to -2.0 pts/sec\n" +
                        "• Proper engine brake use: +0.3 pts/sec",
                    CurrentPenalties = GetSafetyCurrentPenalties(),
                    CurrentBonuses = GetSafetyCurrentBonuses(),
                    ImprovementTips = "• Always use turn signals before turning\n" +
                        "• Release parking brake before driving\n" +
                        "• Use low beams in urban areas\n" +
                        "• Shift gears to avoid over-revving\n" +
                        "• Use engine brake on downhills\n" +
                        "• Avoid riding the brakes"
                };
            }
        }

        /// <summary>
        /// Gets detailed tooltip information for the overall score metric.
        /// </summary>
        public MetricTooltipInfo GetOverallTooltip()
        {
            lock (_lock)
            {
                float damageStreakMinutes = (float)_currentMetrics.DamageFreeStreak.TotalMinutes;
                return new MetricTooltipInfo
                {
                    MetricName = "Overall Performance",
                    Icon = "✨",
                    CurrentValue = _currentMetrics.OverallScore,
                    Weight = 100.0f,
                    CalculationExplanation = "Weighted average of all performance metrics.\n\n" +
                        $"• Smoothness: {_currentMetrics.SmoothnessScore:F1}% (25% weight)\n" +
                        $"• Speed Compliance: {_currentMetrics.SpeedCompliancePercent:F1}% (25% weight)\n" +
                        $"• Safety: {_currentMetrics.SafetyScore:F1}% (25% weight)\n" +
                        $"• Damage-Free: {damageStreakMinutes:F0} min (25% weight)\n\n" +
                        "Grade Scale:\n" +
                        "A+ (95-100), A (90-94), B+ (85-89), B (80-84)\n" +
                        "C+ (75-79), C (70-74), D+ (65-69), D (60-64), F (<60)",
                    CurrentPenalties = "See individual metrics for details",
                    CurrentBonuses = "See individual metrics for details",
                    ImprovementTips = "• Focus on your lowest-scoring metric\n" +
                        "• Drive smoothly and predictably\n" +
                        "• Follow speed limits consistently\n" +
                        "• Use all safety equipment properly\n" +
                        "• Avoid collisions and damage"
                };
            }
        }

        private string GetSmoothnessCurrentPenalties()
        {
            if (_currentMetrics.SmoothnessScore >= 90.0f)
                return "None - Excellent smooth driving!";
            if (_currentMetrics.SmoothnessScore >= 75.0f)
                return "Minor penalties from occasional harsh inputs";
            if (_currentMetrics.SmoothnessScore >= 60.0f)
                return "Moderate penalties from frequent harsh acceleration/braking";
            return "Significant penalties from very harsh driving inputs";
        }

        private string GetSmoothnessCurrentBonuses()
        {
            if (_currentMetrics.SmoothnessScore >= 90.0f)
                return "Earning bonuses for smooth, steady driving";
            if (_currentMetrics.SmoothnessScore >= 75.0f)
                return "Some bonuses for smooth sections";
            return "Limited bonuses - focus on smoother inputs";
        }

        private string GetSpeedComplianceCurrentPenalties()
        {
            if (_currentMetrics.SpeedCompliancePercent >= 95.0f)
                return "None - Excellent speed limit adherence!";
            if (_currentMetrics.SpeedCompliancePercent >= 80.0f)
                return "Minor time spent over speed limit";
            if (_currentMetrics.SpeedCompliancePercent >= 60.0f)
                return "Frequent speeding detected";
            return "Significant time spent over speed limit";
        }

        private string GetSpeedComplianceCurrentBonuses()
        {
            if (_currentMetrics.SpeedCompliancePercent >= 95.0f)
                return "Maintaining excellent compliance";
            return "Stay at or below speed limit to improve";
        }

        private string GetSafetyCurrentPenalties()
        {
            if (_currentMetrics.SafetyScore >= 90.0f)
                return "None - Excellent safety practices!";
            if (_currentMetrics.SafetyScore >= 75.0f)
                return "Minor safety issues detected";
            if (_currentMetrics.SafetyScore >= 60.0f)
                return "Multiple safety violations";
            return "Significant safety concerns detected";
        }

        private string GetSafetyCurrentBonuses()
        {
            if (_currentMetrics.SafetyScore >= 90.0f)
                return "Proper use of signals, lights, and brakes";
            if (_currentMetrics.SafetyScore >= 75.0f)
                return "Some good safety practices observed";
            return "Focus on using safety equipment properly";
        }

        /// <summary>
        /// Updates performance calculations based on new telemetry data.
        /// </summary>
        public void UpdateFromTelemetry(TelemetryData data)
        {
            if (data == null || !data.IsConnected)
            {
                return;
            }

            // Don't update metrics when game is paused
            if (data.IsPaused)
            {
                return;
            }

            lock (_lock)
            {
                // Initialize on first update
                if (_state.PreviousTimestamp == DateTime.MinValue)
                {
                    InitializeState(data);
                    return;
                }

                float deltaTime = (float)(data.Timestamp - _state.PreviousTimestamp).TotalSeconds;

                // Skip if time delta is invalid
                if (deltaTime <= 0 || deltaTime > 10)
                {
                    _state.PreviousTimestamp = data.Timestamp;
                    return;
                }

                // Update all metrics
                UpdateSmoothnessScore(data, deltaTime);
                UpdateFuelEfficiency(data);
                UpdateSpeedCompliance(data, deltaTime);
                UpdateSafetyScore(data, deltaTime);
                UpdateDamageFreeStreak(data, deltaTime);
                UpdateSessionStatistics(data, deltaTime);
                UpdateOverallGrade();

                // Store current values for next iteration
                _state.PreviousSpeed = data.Speed;
                _state.PreviousHeading = data.Heading;
                _state.PreviousPitch = data.Pitch;
                _state.PreviousRoll = data.Roll;
                _state.PreviousTimestamp = data.Timestamp;
            }
        }

        /// <summary>
        /// Resets all session statistics and scores to initial values.
        /// </summary>
        public void ResetSession()
        {
            lock (_lock)
            {
                _state = new CalculationState
                {
                    SessionStartTime = DateTime.Now,
                    DamageFreeStartTime = DateTime.Now,
                    PreviousTimestamp = DateTime.MinValue,
                    SmoothnessScore = 100.0f,
                    SafetyScore = 100.0f,
                    OverallScoreHistory = new List<float>()
                };

                _currentMetrics = new PerformanceMetrics
                {
                    SmoothnessScore = 100.0f,
                    FuelEfficiencyMPG = 0.0f,
                    FuelEfficiencyScore = 0.0f,
                    SpeedCompliancePercent = 100.0f,
                    SpeedComplianceGrade = "A+",
                    SafetyScore = 100.0f,
                    DamageFreeStreak = TimeSpan.Zero,
                    DamageFreeScore = 0.0f,
                    OverallGrade = "A+",
                    OverallScore = 100.0f,
                    Trend = TrendIndicator.Stable,
                    SessionDurationMinutes = 0.0f,
                    SessionDistanceMiles = 0.0f,
                    SessionAverageSpeed = 0.0f,
                    SessionFuelConsumed = 0.0f
                };
            }
        }

        #region Private Helper Methods

        private void InitializeState(TelemetryData data)
        {
            _state.PreviousSpeed = data.Speed;
            _state.PreviousHeading = data.Heading;
            _state.PreviousPitch = data.Pitch;
            _state.PreviousRoll = data.Roll;
            _state.PreviousTimestamp = data.Timestamp;
            _state.SessionStartFuel = data.FuelAmount;
            _state.SessionStartOdometer = data.Odometer;
            _state.SessionStartTime = data.Timestamp;
            _state.LastDamagePercent = data.DamagePercent;
            _state.DamageFreeStartTime = data.Timestamp;
        }

        /// <summary>
        /// Updates smoothness score based on acceleration, braking, and steering patterns.
        /// Requirements: 1.1, 1.2, 1.3, 1.4, 1.5
        /// </summary>
        private void UpdateSmoothnessScore(TelemetryData data, float deltaTime)
        {
            // Convert speed to MPH (assuming telemetry sends km/h)
            float currentSpeedMPH = data.Speed * 0.621371f;
            float previousSpeedMPH = _state.PreviousSpeed * 0.621371f;

            float speedChange = Math.Abs(currentSpeedMPH - previousSpeedMPH);
            float accelerationMPH = speedChange / deltaTime;

            // Calculate heading change (in radians)
            float headingDiff = data.Heading - _state.PreviousHeading;

            // Normalize to shortest angular distance (handle wraparound)
            while (headingDiff > Math.PI) headingDiff -= (float)(2 * Math.PI);
            while (headingDiff < -Math.PI) headingDiff += (float)(2 * Math.PI);

            float headingChange = Math.Abs(headingDiff);
            float headingChangePerSec = headingChange / deltaTime;
            float headingChangeDegPerSec = headingChangePerSec * (180.0f / (float)Math.PI);

            // Track speed changes (acceleration/braking) - slightly more forgiving
            if (currentSpeedMPH > previousSpeedMPH)
            {
                // Acceleration - penalize harsh acceleration
                if (accelerationMPH > 12.0f)
                {
                    _state.SmoothnessScore -= 1.5f;
                    RaiseNotification($"Harsh acceleration -{1.5f:F1} pts", -1.5f, "Smoothness");
                }
                else if (accelerationMPH > 8.0f)
                {
                    _state.SmoothnessScore -= 0.4f;
                    RaiseNotification($"Moderate acceleration -{0.4f:F1} pts", -0.4f, "Smoothness");
                }
                else
                {
                    // Smooth acceleration - small reward
                    float reward = 0.2f * deltaTime;
                    _state.SmoothnessScore += reward;
                    if (reward > 0.1f)
                    {
                        RaiseNotification($"Smooth acceleration +{reward:F1} pts", reward, "Smoothness");
                    }
                }
            }
            else if (currentSpeedMPH < previousSpeedMPH)
            {
                // Deceleration - penalize harsh braking
                if (accelerationMPH > 18.0f)
                {
                    _state.SmoothnessScore -= 3.0f;
                    RaiseNotification($"Harsh braking -{3.0f:F1} pts", -3.0f, "Smoothness");
                }
                else if (accelerationMPH > 12.0f)
                {
                    _state.SmoothnessScore -= 1.0f;
                    RaiseNotification($"Hard braking -{1.0f:F1} pts", -1.0f, "Smoothness");
                }
                else
                {
                    // Smooth braking - small reward
                    float reward = 0.2f * deltaTime;
                    _state.SmoothnessScore += reward;
                    if (reward > 0.1f)
                    {
                        RaiseNotification($"Smooth braking +{reward:F1} pts", reward, "Smoothness");
                    }
                }
            }
            else
            {
                // Constant speed - reward smooth driving
                float reward = 0.4f * deltaTime;
                _state.SmoothnessScore += reward;
            }

            // Track steering changes (swerving) - only when moving, slightly more forgiving
            if (currentSpeedMPH > 10.0f)
            {
                if (headingChangeDegPerSec > 3.5f) // Sharp swerve for a truck
                {
                    _state.SmoothnessScore -= 1.5f;
                    RaiseNotification($"Sharp swerve -{1.5f:F1} pts", -1.5f, "Smoothness");
                }
                else if (headingChangeDegPerSec > 2.5f) // Moderate swerve
                {
                    _state.SmoothnessScore -= 0.8f;
                    RaiseNotification($"Moderate swerve -{0.8f:F1} pts", -0.8f, "Smoothness");
                }
                else if (headingChangeDegPerSec > 1.5f) // Gentle turn
                {
                    _state.SmoothnessScore -= 0.2f;
                }
            }

            // Track pitch and roll changes (bumps, tilting)
            float pitchChange = Math.Abs(data.Pitch - _state.PreviousPitch);
            float rollChange = Math.Abs(data.Roll - _state.PreviousRoll);
            float pitchChangeDegPerSec = (pitchChange / deltaTime) * (180.0f / (float)Math.PI);
            float rollChangeDegPerSec = (rollChange / deltaTime) * (180.0f / (float)Math.PI);

            // Penalize excessive pitch/roll changes (rough driving, hitting bumps hard)
            if (currentSpeedMPH > 5.0f)
            {
                if (pitchChangeDegPerSec > 15.0f || rollChangeDegPerSec > 15.0f)
                {
                    _state.SmoothnessScore -= 0.8f;
                }
            }

            // Clamp to 0-100 range
            _state.SmoothnessScore = ScoreHelper.ClampScore(_state.SmoothnessScore);
            _currentMetrics.SmoothnessScore = _state.SmoothnessScore;
        }

        /// <summary>
        /// Updates fuel efficiency metrics based on driving behavior.
        /// Since telemetry doesn't provide fuel data, estimate based on smoothness.
        /// Requirements: 2.1, 2.2, 2.3, 2.4, 2.5
        /// </summary>
        private void UpdateFuelEfficiency(TelemetryData data)
        {
            // Calculate distance traveled in miles (odometer is in km)
            float distanceKm = data.Odometer - _state.SessionStartOdometer;
            float distanceMiles = distanceKm * 0.621371f;

            _currentMetrics.SessionDistanceMiles = distanceMiles;
            _currentMetrics.SessionFuelConsumed = 0; // Not available from telemetry

            // Estimate MPG based on smoothness score
            // Smooth driving (high score) = better fuel efficiency
            // Range: 4.0 MPG (poor) to 8.0 MPG (excellent)
            float estimatedMPG = 4.0f + (_state.SmoothnessScore / 100.0f) * 4.0f;

            _currentMetrics.FuelEfficiencyMPG = estimatedMPG;
            _currentMetrics.FuelEfficiencyScore = (estimatedMPG / 6.0f) * 100.0f;
        }

        /// <summary>
        /// Updates safety score based on proper use of signals, lights, brakes, and other safety practices.
        /// Requirements: Safety and compliance tracking
        /// </summary>
        private void UpdateSafetyScore(TelemetryData data, float deltaTime)
        {
            // Convert current speed to MPH
            float currentSpeedMPH = data.Speed * 0.621371f;

            // Calculate heading change (in radians)
            float headingDiff = data.Heading - _state.PreviousHeading;

            // Normalize to shortest angular distance (handle wraparound)
            while (headingDiff > Math.PI) headingDiff -= (float)(2 * Math.PI);
            while (headingDiff < -Math.PI) headingDiff += (float)(2 * Math.PI);

            float headingChange = Math.Abs(headingDiff);
            float headingChangePerSec = headingChange / deltaTime;
            float headingChangeDegPerSec = headingChangePerSec * (180.0f / (float)Math.PI);

            // Track blinker usage during turns (detect heading changes without blinkers active)
            // Only check when moving at reasonable speed
            if (currentSpeedMPH > 5.0f)
            {
                // Significant turn detected (> 15 degrees per second)
                if (headingChangeDegPerSec > 15.0f)
                {
                    bool isTurningLeft = headingDiff < 0;
                    bool isTurningRight = headingDiff > 0;

                    // Penalize if turning without appropriate blinker
                    if (isTurningLeft && !data.BlinkerLeftActive)
                    {
                        _state.SafetyScore -= 2.0f;
                        RaiseNotification("Turn without blinker -2.0 pts", -2.0f, "Safety");
                    }
                    else if (isTurningRight && !data.BlinkerRightActive)
                    {
                        _state.SafetyScore -= 2.0f;
                        RaiseNotification("Turn without blinker -2.0 pts", -2.0f, "Safety");
                    }
                }
            }

            // Penalize driving with parking brake engaged (when moving)
            if (currentSpeedMPH > 1.0f && data.ParkBrakeOn)
            {
                float penalty = 5.0f * deltaTime;
                _state.SafetyScore -= penalty;
                if (penalty > 0.5f)
                {
                    RaiseNotification($"Parking brake engaged -{penalty:F1} pts", -penalty, "Safety");
                }
            }

            // Penalize inappropriate high beam usage
            // High beams should not be used in urban areas (speed < 45 MPH) or when speed limit is low
            if (data.LightsBeamHighOn)
            {
                float speedLimit = DetermineSpeedLimit(data);
                if (currentSpeedMPH < 45.0f || speedLimit < 55.0f)
                {
                    float penalty = 0.5f * deltaTime;
                    _state.SafetyScore -= penalty;
                    // Only notify occasionally to avoid spam
                    if (_state.HighBeamWarningCooldown <= 0)
                    {
                        RaiseNotification("Inappropriate high beam use", -penalty, "Safety");
                        _state.HighBeamWarningCooldown = 5.0f; // 5 second cooldown
                    }
                    _state.HighBeamWarningCooldown -= deltaTime;
                }
            }

            // Reward proper engine brake/retarder usage
            // Engine brake is good for controlling speed on downhills without overheating brakes
            if (data.MotorBrakeOn || data.RetarderBrake > 0)
            {
                // Check if we're going downhill (negative pitch) and at reasonable speed
                if (data.Pitch < -0.05f && currentSpeedMPH > 20.0f)
                {
                    float reward = 0.3f * deltaTime;
                    _state.SafetyScore += reward;
                    // Only notify occasionally to avoid spam
                    if (_state.EngineBrakeRewardCooldown <= 0 && reward > 0.1f)
                    {
                        RaiseNotification($"Good engine brake use +{reward:F1} pts", reward, "Safety");
                        _state.EngineBrakeRewardCooldown = 10.0f; // 10 second cooldown
                    }
                    _state.EngineBrakeRewardCooldown -= deltaTime;
                }
            }

            // Penalize over-revving engine (RPM > 90% of max)
            if (data.EngineRpmMax > 0)
            {
                float rpmPercent = (data.EngineRpm / data.EngineRpmMax) * 100.0f;
                if (rpmPercent > 90.0f)
                {
                    float penalty = 1.0f * deltaTime;
                    _state.SafetyScore -= penalty;
                    // Only notify occasionally to avoid spam
                    if (_state.OverRevWarningCooldown <= 0)
                    {
                        RaiseNotification("Over-revving engine", -penalty, "Safety");
                        _state.OverRevWarningCooldown = 3.0f; // 3 second cooldown
                    }
                    _state.OverRevWarningCooldown -= deltaTime;
                }
            }

            // Penalize excessive brake temperature (> 200°C indicates poor braking technique)
            if (data.BrakeTemperature > 200.0f)
            {
                // More severe penalty for very hot brakes
                if (data.BrakeTemperature > 300.0f)
                {
                    float penalty = 2.0f * deltaTime;
                    _state.SafetyScore -= penalty;
                    if (_state.BrakeTempWarningCooldown <= 0)
                    {
                        RaiseNotification($"Brakes overheating ({data.BrakeTemperature:F0}°C)", -penalty, "Safety");
                        _state.BrakeTempWarningCooldown = 5.0f;
                    }
                    _state.BrakeTempWarningCooldown -= deltaTime;
                }
                else
                {
                    float penalty = 1.0f * deltaTime;
                    _state.SafetyScore -= penalty;
                    if (_state.BrakeTempWarningCooldown <= 0)
                    {
                        RaiseNotification($"Brakes hot ({data.BrakeTemperature:F0}°C)", -penalty, "Safety");
                        _state.BrakeTempWarningCooldown = 5.0f;
                    }
                    _state.BrakeTempWarningCooldown -= deltaTime;
                }
            }

            // Clamp to 0-100 range
            _state.SafetyScore = ScoreHelper.ClampScore(_state.SafetyScore);
            _currentMetrics.SafetyScore = _state.SafetyScore;
        }

        /// <summary>
        /// Updates speed compliance metrics based on speed limit adherence.
        /// Requirements: 3.1, 3.2, 3.3, 3.4, 3.5
        /// </summary>
        private void UpdateSpeedCompliance(TelemetryData data, float deltaTime)
        {
            // Determine speed limit (65 MPH highway, 55 MPH default)
            float speedLimit = DetermineSpeedLimit(data);

            // Convert current speed to MPH (assuming telemetry sends km/h)
            float currentSpeedMPH = data.Speed * 0.621371f;

            // Track time spent compliant
            if (currentSpeedMPH <= speedLimit)
            {
                _state.TimeCompliant += deltaTime;
            }
            _state.TotalTime += deltaTime;

            // Calculate compliance percentage
            if (_state.TotalTime > 0)
            {
                _currentMetrics.SpeedCompliancePercent = (_state.TimeCompliant / _state.TotalTime) * 100.0f;
                _currentMetrics.SpeedComplianceGrade = ScoreHelper.ConvertScoreToGrade(_currentMetrics.SpeedCompliancePercent);
            }
        }

        /// <summary>
        /// Determines the applicable speed limit based on telemetry data.
        /// </summary>
        private float DetermineSpeedLimit(TelemetryData data)
        {
            // Use telemetry speed limit if available (already in km/h, convert to MPH)
            if (data.SpeedLimit > 0)
            {
                return data.SpeedLimit * 0.621371f;
            }

            // Default: assume highway (65 MPH) if speed > 50 MPH, otherwise 55 MPH
            float currentSpeedMPH = data.Speed * 0.621371f;
            return currentSpeedMPH > 50.0f ? 65.0f : 55.0f;
        }

        /// <summary>
        /// Updates damage-free streak tracking.
        /// Requirements: 4.1, 4.2, 4.3, 4.4
        /// </summary>
        private void UpdateDamageFreeStreak(TelemetryData data, float deltaTime)
        {
            // Check if damage increased (with small threshold to avoid floating point issues)
            // Even tiny damage should reset the streak
            if (data.DamagePercent > _state.LastDamagePercent + 0.001f)
            {
                // Reset streak
                var previousStreak = _currentMetrics.DamageFreeStreak;
                _state.DamageFreeStartTime = data.Timestamp;
                _currentMetrics.DamageFreeStreak = TimeSpan.Zero;

                // Notify about damage
                if (previousStreak.TotalMinutes > 1.0)
                {
                    RaiseNotification($"Damage! Streak reset ({previousStreak.Hours:D2}:{previousStreak.Minutes:D2})", 0, "Damage");
                }
                else
                {
                    RaiseNotification("Vehicle damaged", 0, "Damage");
                }
            }
            else
            {
                // Increment streak
                _currentMetrics.DamageFreeStreak = data.Timestamp - _state.DamageFreeStartTime;
            }

            _state.LastDamagePercent = data.DamagePercent;
        }

        /// <summary>
        /// Updates session statistics (duration, distance, average speed).
        /// </summary>
        private void UpdateSessionStatistics(TelemetryData data, float deltaTime)
        {
            // Calculate session duration
            TimeSpan sessionDuration = data.Timestamp - _state.SessionStartTime;
            _currentMetrics.SessionDurationMinutes = (float)sessionDuration.TotalMinutes;

            // Calculate average speed
            if (_currentMetrics.SessionDurationMinutes > 0)
            {
                _currentMetrics.SessionAverageSpeed = (_currentMetrics.SessionDistanceMiles / _currentMetrics.SessionDurationMinutes) * 60.0f;
            }
        }

        /// <summary>
        /// Updates overall grade and trend indicator.
        /// Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6
        /// Weight: 25% smoothness, 25% speed compliance, 25% safety, 25% vehicle condition (inverse of damage)
        /// </summary>
        private void UpdateOverallGrade()
        {
            // Calculate weighted average of all four metrics
            float smoothness = _currentMetrics.SmoothnessScore;
            float speedCompliance = _currentMetrics.SpeedCompliancePercent;
            float safety = _currentMetrics.SafetyScore;

            // Calculate vehicle condition score (inverse of damage percentage)
            // 0% damage = 100% condition, 100% damage = 0% condition
            float vehicleConditionScore = 100.0f - _state.LastDamagePercent;
            vehicleConditionScore = Math.Max(0, Math.Min(100, vehicleConditionScore));

            // Also calculate damage-free streak score for the breakdown display
            float damageStreakMinutes = (float)_currentMetrics.DamageFreeStreak.TotalMinutes;
            float damageStreakScore = 0.0f;
            if (damageStreakMinutes >= 60.0f)
            {
                damageStreakScore = 100.0f;
            }
            else if (damageStreakMinutes >= 30.0f)
            {
                damageStreakScore = 75.0f + ((damageStreakMinutes - 30.0f) / 30.0f) * 25.0f;
            }
            else if (damageStreakMinutes >= 15.0f)
            {
                damageStreakScore = 50.0f + ((damageStreakMinutes - 15.0f) / 15.0f) * 25.0f;
            }
            else if (damageStreakMinutes >= 5.0f)
            {
                damageStreakScore = 25.0f + ((damageStreakMinutes - 5.0f) / 10.0f) * 25.0f;
            }
            else
            {
                damageStreakScore = (damageStreakMinutes / 5.0f) * 25.0f;
            }

            // Store the damage-free streak score (for breakdown display)
            _currentMetrics.DamageFreeScore = damageStreakScore;

            // Calculate weighted average: 25% each, using vehicle condition (not streak)
            _currentMetrics.OverallScore = (smoothness * 0.25f) + (speedCompliance * 0.25f) + (safety * 0.25f) + (vehicleConditionScore * 0.25f);
            _currentMetrics.OverallGrade = ScoreHelper.ConvertScoreToGrade(_currentMetrics.OverallScore);

            // Track historical data for sparklines (last 60 seconds)
            _state.SmoothnessHistory.Add(smoothness);
            _state.SpeedComplianceHistory.Add(speedCompliance);
            _state.SafetyHistory.Add(safety);
            _state.OverallScoreHistory.Add(_currentMetrics.OverallScore);

            // Keep only recent history (last 60 seconds worth of data at ~10 Hz = 600 samples)
            const int maxHistorySize = 600;
            if (_state.SmoothnessHistory.Count > maxHistorySize)
            {
                _state.SmoothnessHistory.RemoveAt(0);
            }
            if (_state.SpeedComplianceHistory.Count > maxHistorySize)
            {
                _state.SpeedComplianceHistory.RemoveAt(0);
            }
            if (_state.SafetyHistory.Count > maxHistorySize)
            {
                _state.SafetyHistory.RemoveAt(0);
            }
            if (_state.OverallScoreHistory.Count > maxHistorySize)
            {
                _state.OverallScoreHistory.RemoveAt(0);
            }

            // Calculate trend (compare current to session average)
            if (_state.OverallScoreHistory.Count > 10)
            {
                float sessionAverage = _state.OverallScoreHistory.Average();
                float difference = _currentMetrics.OverallScore - sessionAverage;

                if (difference > 2.0f)
                {
                    _currentMetrics.Trend = TrendIndicator.Up;
                }
                else if (difference < -2.0f)
                {
                    _currentMetrics.Trend = TrendIndicator.Down;
                }
                else
                {
                    _currentMetrics.Trend = TrendIndicator.Stable;
                }
            }
            else
            {
                _currentMetrics.Trend = TrendIndicator.Stable;
            }
        }



        /// <summary>
        /// Updates the score breakdown with current component values and contributions.
        /// </summary>
        private void UpdateScoreBreakdown()
        {
            // Calculate vehicle condition score (inverse of damage percentage)
            float vehicleConditionScore = ScoreHelper.ClampScore(100.0f - _state.LastDamagePercent);

            // Also calculate damage-free streak for informational purposes
            float damageStreakMinutes = (float)_currentMetrics.DamageFreeStreak.TotalMinutes;

            // Smoothness component
            _currentBreakdown.Smoothness.Name = "Smoothness";
            _currentBreakdown.Smoothness.Icon = "🎯";
            _currentBreakdown.Smoothness.Value = _currentMetrics.SmoothnessScore;
            _currentBreakdown.Smoothness.ContributionPercent = 25.0f;
            _currentBreakdown.Smoothness.Description = ScoreHelper.GetSmoothnessDescription(_currentMetrics.SmoothnessScore);
            _currentBreakdown.Smoothness.ColorIndicator = ScoreHelper.GetColorIndicator(_currentMetrics.SmoothnessScore);

            // Speed Compliance component
            _currentBreakdown.SpeedCompliance.Name = "Speed Compliance";
            _currentBreakdown.SpeedCompliance.Icon = "🚦";
            _currentBreakdown.SpeedCompliance.Value = _currentMetrics.SpeedCompliancePercent;
            _currentBreakdown.SpeedCompliance.ContributionPercent = 25.0f;
            _currentBreakdown.SpeedCompliance.Description = ScoreHelper.GetSpeedComplianceDescription(_currentMetrics.SpeedCompliancePercent);
            _currentBreakdown.SpeedCompliance.ColorIndicator = ScoreHelper.GetColorIndicator(_currentMetrics.SpeedCompliancePercent);

            // Safety component
            _currentBreakdown.Safety.Name = "Safety";
            _currentBreakdown.Safety.Icon = "🛡️";
            _currentBreakdown.Safety.Value = _currentMetrics.SafetyScore;
            _currentBreakdown.Safety.ContributionPercent = 25.0f;
            _currentBreakdown.Safety.Description = ScoreHelper.GetSafetyDescription(_currentMetrics.SafetyScore);
            _currentBreakdown.Safety.ColorIndicator = ScoreHelper.GetColorIndicator(_currentMetrics.SafetyScore);

            // Vehicle Condition component (inverse of damage)
            _currentBreakdown.DamageFree.Name = "Vehicle Condition";
            _currentBreakdown.DamageFree.Icon = "🔧";
            _currentBreakdown.DamageFree.Value = vehicleConditionScore;
            _currentBreakdown.DamageFree.ContributionPercent = 25.0f;
            _currentBreakdown.DamageFree.Description = ScoreHelper.GetVehicleConditionDescription(_state.LastDamagePercent, damageStreakMinutes);
            _currentBreakdown.DamageFree.ColorIndicator = ScoreHelper.GetColorIndicator(vehicleConditionScore);
        }



        #endregion

        #region Internal State Class

        /// <summary>
        /// Internal state for performance calculations.
        /// </summary>
        private class CalculationState
        {
            // For smoothness calculation
            public float PreviousSpeed { get; set; }
            public float PreviousHeading { get; set; }
            public float PreviousPitch { get; set; }
            public float PreviousRoll { get; set; }
            public DateTime PreviousTimestamp { get; set; }
            public float SmoothnessScore { get; set; }

            // For fuel efficiency
            public float SessionStartFuel { get; set; }
            public float SessionStartOdometer { get; set; }

            // For speed compliance
            public float TimeCompliant { get; set; }
            public float TotalTime { get; set; }

            // For safety score
            public float SafetyScore { get; set; }

            // For damage tracking
            public float LastDamagePercent { get; set; }
            public DateTime DamageFreeStartTime { get; set; }

            // For session tracking
            public DateTime SessionStartTime { get; set; }

            // For trend calculation
            public List<float> OverallScoreHistory { get; set; } = new List<float>();

            // For sparkline visualization (last 60 seconds)
            public List<float> SmoothnessHistory { get; set; } = new List<float>();
            public List<float> SpeedComplianceHistory { get; set; } = new List<float>();
            public List<float> SafetyHistory { get; set; } = new List<float>();

            // Notification cooldowns (to prevent spam)
            public float HighBeamWarningCooldown { get; set; }
            public float EngineBrakeRewardCooldown { get; set; }
            public float OverRevWarningCooldown { get; set; }
            public float BrakeTempWarningCooldown { get; set; }
        }

        #endregion
    }
}
