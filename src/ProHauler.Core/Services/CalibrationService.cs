using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using ProHauler.Core.Interfaces;
using ProHauler.Core.Models;

namespace ProHauler.Core.Services
{
    /// <summary>
    /// Service for loading and applying calibration data to coordinate projection.
    /// </summary>
    public class CalibrationService
    {
        private readonly ICoordinateProjection _projection;
        private List<CalibrationPoint>? _calibrationPoints;

        public CalibrationService(ICoordinateProjection projection)
        {
            _projection = projection ?? throw new ArgumentNullException(nameof(projection));
        }

        /// <summary>
        /// Loads calibration points from a JSON file.
        /// </summary>
        /// <param name="filePath">Path to the calibration.json file</param>
        /// <returns>List of calibration points</returns>
        public List<CalibrationPoint> LoadCalibrationPoints(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Calibration file not found at: {filePath}", filePath);
            }

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                var calibrationData = JsonSerializer.Deserialize<CalibrationData>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (calibrationData?.ReferencePoints == null || calibrationData.ReferencePoints.Count == 0)
                {
                    throw new InvalidDataException("Calibration file contains no reference points");
                }

                _calibrationPoints = new List<CalibrationPoint>();

                foreach (var point in calibrationData.ReferencePoints)
                {
                    _calibrationPoints.Add(new CalibrationPoint
                    {
                        LocationName = point.Name,
                        WorldPosition = new Vector3(
                            (float)point.WorldPosition.X,
                            (float)point.WorldPosition.Y,
                            (float)point.WorldPosition.Z
                        ),
                        MapPixelPosition = new Point(
                            point.MapPixelPosition.X,
                            point.MapPixelPosition.Y
                        )
                    });
                }

                Console.WriteLine($"Loaded {_calibrationPoints.Count} calibration points from {filePath}");
                return _calibrationPoints;
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"Failed to parse calibration file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Applies the loaded calibration points to the coordinate projection.
        /// </summary>
        public void ApplyCalibration()
        {
            if (_calibrationPoints == null || _calibrationPoints.Count == 0)
            {
                throw new InvalidOperationException("No calibration points loaded. Call LoadCalibrationPoints first.");
            }

            _projection.Calibrate(_calibrationPoints);
            Console.WriteLine("Calibration applied successfully");
        }

        /// <summary>
        /// Loads calibration points and applies them to the projection in one step.
        /// </summary>
        /// <param name="filePath">Path to the calibration.json file</param>
        public void LoadAndApplyCalibration(string filePath)
        {
            LoadCalibrationPoints(filePath);
            ApplyCalibration();
        }

        /// <summary>
        /// Logs the calibration accuracy for each reference point.
        /// </summary>
        public void LogCalibrationAccuracy()
        {
            if (_calibrationPoints == null || _calibrationPoints.Count == 0)
            {
                Console.WriteLine("No calibration points to check accuracy");
                return;
            }

            if (_projection is AffineCoordinateProjection affineProjection)
            {
                var accuracy = affineProjection.GetCalibrationAccuracy(_calibrationPoints);

                Console.WriteLine("\n=== Calibration Accuracy Report ===");

                double totalError = 0;
                foreach (var kvp in accuracy)
                {
                    Console.WriteLine($"{kvp.Key}: {kvp.Value:F2} pixels error");
                    totalError += kvp.Value;
                }

                double averageError = totalError / accuracy.Count;
                Console.WriteLine($"Average error: {averageError:F2} pixels");
                Console.WriteLine("===================================\n");

                // Warn if accuracy is poor
                if (averageError > 50)
                {
                    Console.WriteLine($"WARNING: Average calibration error ({averageError:F2} pixels) exceeds 50 pixels.");
                    Console.WriteLine("Consider verifying calibration point coordinates.");
                }
            }
        }

        /// <summary>
        /// Gets the loaded calibration points.
        /// </summary>
        public List<CalibrationPoint>? GetCalibrationPoints()
        {
            return _calibrationPoints;
        }
    }

    /// <summary>
    /// Internal class for JSON deserialization of calibration data.
    /// </summary>
    internal class CalibrationData
    {
        public List<CalibrationPointJson> ReferencePoints { get; set; } = new();
    }

    internal class CalibrationPointJson
    {
        public string Name { get; set; } = string.Empty;
        public WorldPositionJson WorldPosition { get; set; } = new();
        public MapPixelPositionJson MapPixelPosition { get; set; } = new();
    }

    internal class WorldPositionJson
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    internal class MapPixelPositionJson
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
