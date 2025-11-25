using System;
using System.Collections.Generic;
using System.Windows;
using ProHauler.Core.Interfaces;
using ProHauler.Core.Models;
using MathNet.Numerics.LinearAlgebra;

namespace ProHauler.Core.Services
{
    /// <summary>
    /// Implements coordinate projection using affine transformation.
    /// Converts ATS 3D world coordinates to 2D map pixel coordinates.
    /// </summary>
    public class AffineCoordinateProjection : ICoordinateProjection
    {
        // Transformation matrix coefficients
        // [x_map]   [a  b] [x_world]   [tx]
        // [y_map] = [c  d] [z_world] + [ty]
        private double _a, _b, _c, _d, _tx, _ty;

        // Map bounds for validation
        private double _mapWidth;
        private double _mapHeight;

        private bool _isCalibrated = false;

        /// <summary>
        /// Calibrates the projection using known reference points.
        /// Uses least-squares solution to find optimal affine transformation.
        /// </summary>
        /// <param name="referencePoints">List of calibration points with known world and map positions</param>
        public void Calibrate(List<CalibrationPoint> referencePoints)
        {
            if (referencePoints == null || referencePoints.Count < 3)
            {
                throw new ArgumentException("At least 3 calibration points are required", nameof(referencePoints));
            }

            int n = referencePoints.Count;

            // Build matrices for least-squares solution: A * x = b
            // where x = [a, b, tx, c, d, ty]
            var matrixA = Matrix<double>.Build.Dense(2 * n, 6);
            var vectorB = Vector<double>.Build.Dense(2 * n);

            for (int i = 0; i < n; i++)
            {
                var wp = referencePoints[i].WorldPosition;
                var mp = referencePoints[i].MapPixelPosition;

                // Equation for x_map: a*x + b*z + tx = x_map
                matrixA[2 * i, 0] = wp.X;      // coefficient for a
                matrixA[2 * i, 1] = wp.Z;      // coefficient for b
                matrixA[2 * i, 2] = 1;         // coefficient for tx
                vectorB[2 * i] = mp.X;

                // Equation for y_map: c*x + d*z + ty = y_map
                matrixA[2 * i + 1, 3] = wp.X;  // coefficient for c
                matrixA[2 * i + 1, 4] = wp.Z;  // coefficient for d
                matrixA[2 * i + 1, 5] = 1;     // coefficient for ty
                vectorB[2 * i + 1] = mp.Y;
            }

            // Solve using least squares: x = (A^T * A)^-1 * A^T * b
            var solution = matrixA.TransposeThisAndMultiply(matrixA)
                .Solve(matrixA.TransposeThisAndMultiply(vectorB));

            // Extract transformation parameters
            _a = solution[0];
            _b = solution[1];
            _tx = solution[2];
            _c = solution[3];
            _d = solution[4];
            _ty = solution[5];

            _isCalibrated = true;

            // Calculate and store map bounds from calibration points
            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;

            foreach (var point in referencePoints)
            {
                minX = Math.Min(minX, point.MapPixelPosition.X);
                maxX = Math.Max(maxX, point.MapPixelPosition.X);
                minY = Math.Min(minY, point.MapPixelPosition.Y);
                maxY = Math.Max(maxY, point.MapPixelPosition.Y);
            }

            // Add margin to bounds
            _mapWidth = maxX + 1000;
            _mapHeight = maxY + 1000;
        }

        /// <summary>
        /// Converts ATS world coordinates to map pixel coordinates.
        /// </summary>
        /// <param name="worldPosition">3D position in ATS world (X, Y, Z)</param>
        /// <returns>2D pixel position on map</returns>
        public Point WorldToMap(Vector3 worldPosition)
        {
            if (!_isCalibrated)
            {
                throw new InvalidOperationException("Projection must be calibrated before use. Call Calibrate() first.");
            }

            // Apply affine transformation
            // Note: We use X and Z from world coordinates (Y is altitude, not used for 2D map)
            double x = _a * worldPosition.X + _b * worldPosition.Z + _tx;
            double y = _c * worldPosition.X + _d * worldPosition.Z + _ty;

            // Validate and clamp coordinates to map bounds
            if (x < 0 || x > _mapWidth || y < 0 || y > _mapHeight)
            {
                Console.WriteLine($"Warning: Position out of bounds - World({worldPosition.X:F0}, {worldPosition.Z:F0}) -> Map({x:F0}, {y:F0})");
            }

            // Clamp to map edges
            x = Math.Max(0, Math.Min(_mapWidth, x));
            y = Math.Max(0, Math.Min(_mapHeight, y));

            return new Point(x, y);
        }

        /// <summary>
        /// Converts map pixel coordinates back to ATS world coordinates.
        /// Note: This is an approximate inverse transformation (Y altitude is set to 0).
        /// </summary>
        /// <param name="mapPosition">2D pixel position on map</param>
        /// <returns>3D position in ATS world</returns>
        public Vector3 MapToWorld(Point mapPosition)
        {
            if (!_isCalibrated)
            {
                throw new InvalidOperationException("Projection must be calibrated before use. Call Calibrate() first.");
            }

            // Solve inverse transformation
            // [x_world]   [a  b]^-1  ([x_map]   [tx])
            // [z_world] = [c  d]     ([y_map] - [ty])

            double determinant = _a * _d - _b * _c;

            if (Math.Abs(determinant) < 1e-10)
            {
                throw new InvalidOperationException("Transformation matrix is singular, cannot compute inverse");
            }

            double xAdj = mapPosition.X - _tx;
            double yAdj = mapPosition.Y - _ty;

            double worldX = (_d * xAdj - _b * yAdj) / determinant;
            double worldZ = (-_c * xAdj + _a * yAdj) / determinant;

            return new Vector3((float)worldX, 0, (float)worldZ);
        }

        /// <summary>
        /// Sets the map dimensions for coordinate validation.
        /// </summary>
        /// <param name="width">Map width in pixels</param>
        /// <param name="height">Map height in pixels</param>
        public void SetMapBounds(double width, double height)
        {
            _mapWidth = width;
            _mapHeight = height;
        }

        /// <summary>
        /// Gets the calibration accuracy by calculating error for each reference point.
        /// </summary>
        /// <param name="referencePoints">The calibration points to test</param>
        /// <returns>Dictionary mapping location name to error in pixels</returns>
        public Dictionary<string, double> GetCalibrationAccuracy(List<CalibrationPoint> referencePoints)
        {
            if (!_isCalibrated)
            {
                throw new InvalidOperationException("Projection must be calibrated before checking accuracy");
            }

            var accuracy = new Dictionary<string, double>();

            foreach (var point in referencePoints)
            {
                var calculated = WorldToMap(point.WorldPosition);
                var expected = point.MapPixelPosition;

                double error = Math.Sqrt(
                    Math.Pow(calculated.X - expected.X, 2) +
                    Math.Pow(calculated.Y - expected.Y, 2)
                );

                accuracy[point.LocationName] = error;
            }

            return accuracy;
        }
    }
}
