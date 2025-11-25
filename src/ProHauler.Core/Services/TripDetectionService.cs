using ProHauler.Core.Interfaces;
using ProHauler.Core.Models;
using Serilog;
using System;
using System.Threading.Tasks;

namespace ProHauler.Core.Services
{
    /// <summary>
    /// Implements automatic trip detection and lifecycle management.
    /// Monitors telemetry to detect when trips start and end, tracks trip statistics,
    /// and persists completed trips to the database.
    /// </summary>
    public class TripDetectionService : ITripDetectionService
    {
        private readonly ITripRepository _tripRepository;
        private readonly IPerformanceCalculator _performanceCalculator;

        private Trip? _currentTrip;
        private DateTime _lastMovementTime;
        private bool _wasMoving;
        private float _lastOdometer;
        private float _lastFuelAmount;
        private Vector3 _lastPosition;

        // Trip end timeout: 5 minutes of no movement
        private static readonly TimeSpan TripEndTimeout = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Initializes a new instance of the TripDetectionService class.
        /// </summary>
        /// <param name="tripRepository">Repository for persisting trip data.</param>
        /// <param name="performanceCalculator">Calculator for capturing performance metrics.</param>
        public TripDetectionService(
            ITripRepository tripRepository,
            IPerformanceCalculator performanceCalculator)
        {
            _tripRepository = tripRepository ?? throw new ArgumentNullException(nameof(tripRepository));
            _performanceCalculator = performanceCalculator ?? throw new ArgumentNullException(nameof(performanceCalculator));

            _wasMoving = false;
            _lastMovementTime = DateTime.UtcNow;
        }

        /// <inheritdoc/>
        public bool IsTripActive => _currentTrip != null;

        /// <inheritdoc/>
        public Trip? CurrentTrip => _currentTrip;

        /// <inheritdoc/>
        public void UpdateFromTelemetry(TelemetryData data)
        {
            if (data == null || !data.IsConnected)
            {
                return;
            }

            bool isMoving = data.Speed > 0.1f; // Consider moving if speed > 0.1 m/s (~0.22 mph)

            // Trip start detection: vehicle starts moving for the first time
            if (isMoving && !_wasMoving && _currentTrip == null)
            {
                StartTrip(data);
            }

            // Update trip data if trip is active
            if (_currentTrip != null)
            {
                UpdateTripData(data, isMoving);
            }

            // Trip end detection: vehicle stopped for more than 5 minutes
            if (_currentTrip != null && !isMoving)
            {
                TimeSpan timeSinceLastMovement = DateTime.UtcNow - _lastMovementTime;
                if (timeSinceLastMovement > TripEndTimeout)
                {
                    Log.Information("Trip end detected: vehicle stopped for {Minutes} minutes",
                        timeSinceLastMovement.TotalMinutes);
                    _ = EndCurrentTripAsync(); // Fire and forget - we'll handle errors in the async method
                }
            }

            // Update tracking state
            if (isMoving)
            {
                _lastMovementTime = DateTime.UtcNow;
            }
            _wasMoving = isMoving;
        }

        /// <inheritdoc/>
        public async Task<Trip?> EndCurrentTripAsync()
        {
            if (_currentTrip == null)
            {
                return null;
            }

            try
            {
                // Capture final performance metrics
                var metrics = _performanceCalculator.GetCurrentMetrics();

                _currentTrip.EndTime = DateTime.UtcNow;
                _currentTrip.DurationMinutes = (float)(_currentTrip.EndTime - _currentTrip.StartTime).TotalMinutes;

                // Capture performance scores
                _currentTrip.SmoothnessScore = metrics.SmoothnessScore;
                _currentTrip.FuelEfficiencyMPG = metrics.FuelEfficiencyMPG;
                _currentTrip.SpeedCompliancePercent = metrics.SpeedCompliancePercent;
                _currentTrip.SafetyScore = metrics.SafetyScore;
                _currentTrip.OverallGrade = metrics.OverallGrade;

                // Calculate average speed
                if (_currentTrip.DurationMinutes > 0)
                {
                    _currentTrip.AverageSpeed = (_currentTrip.DistanceMiles / _currentTrip.DurationMinutes) * 60.0f;
                }

                // Save to database
                int tripId = await _tripRepository.SaveTripAsync(_currentTrip);
                _currentTrip.Id = tripId;

                Log.Information(
                    "Trip completed and saved: ID={TripId}, Duration={Duration}min, Distance={Distance}mi, Grade={Grade}",
                    tripId, _currentTrip.DurationMinutes, _currentTrip.DistanceMiles, _currentTrip.OverallGrade);

                var completedTrip = _currentTrip;
                _currentTrip = null;

                return completedTrip;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save trip to database");

                // Still clear the current trip to avoid data corruption
                var failedTrip = _currentTrip;
                _currentTrip = null;

                return failedTrip;
            }
        }

        /// <summary>
        /// Starts a new trip when vehicle begins moving.
        /// </summary>
        private void StartTrip(TelemetryData data)
        {
            _currentTrip = new Trip
            {
                StartTime = DateTime.UtcNow,
                DistanceMiles = 0,
                FuelConsumed = 0,
                OverallGrade = "N/A"
            };

            // Initialize tracking values
            _lastOdometer = data.Odometer;
            _lastFuelAmount = data.FuelAmount;
            _lastPosition = data.Position;

            Log.Information("Trip started at {StartTime}", _currentTrip.StartTime);
        }

        /// <summary>
        /// Updates trip statistics based on new telemetry data.
        /// </summary>
        private void UpdateTripData(TelemetryData data, bool isMoving)
        {
            if (_currentTrip == null)
            {
                return;
            }

            // Track distance traveled
            // Use odometer if available and reliable, otherwise calculate from position
            if (data.Odometer > 0 && data.Odometer > _lastOdometer)
            {
                float distanceKm = data.Odometer - _lastOdometer;
                float distanceMiles = distanceKm * 0.621371f; // Convert km to miles
                _currentTrip.DistanceMiles += distanceMiles;
                _lastOdometer = data.Odometer;
            }
            else if (isMoving)
            {
                // Fallback: calculate distance from position changes
                float dx = data.Position.X - _lastPosition.X;
                float dy = data.Position.Y - _lastPosition.Y;
                float dz = data.Position.Z - _lastPosition.Z;
                float distanceMeters = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
                float distanceMiles = distanceMeters * 0.000621371f; // Convert meters to miles

                // Only add if movement is reasonable (< 200 mph to filter out teleports/glitches)
                if (distanceMiles < 0.1f) // Less than 0.1 miles per update
                {
                    _currentTrip.DistanceMiles += distanceMiles;
                }

                _lastPosition = data.Position;
            }

            // Track fuel consumption
            if (data.FuelAmount > 0 && data.FuelAmount < _lastFuelAmount)
            {
                float fuelLiters = _lastFuelAmount - data.FuelAmount;
                float fuelGallons = fuelLiters * 0.264172f; // Convert liters to gallons
                _currentTrip.FuelConsumed += fuelGallons;
            }
            _lastFuelAmount = data.FuelAmount;
        }
    }
}
