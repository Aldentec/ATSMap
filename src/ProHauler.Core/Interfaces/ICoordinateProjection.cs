using System.Collections.Generic;
using System.Windows;
using ProHauler.Core.Models;

namespace ProHauler.Core.Interfaces
{
    public interface ICoordinateProjection
    {
        // Convert 3D world position to 2D map pixel position
        Point WorldToMap(Vector3 worldPosition);

        // Convert 2D map pixel position back to 3D world position
        Vector3 MapToWorld(Point mapPosition);

        // Set up the transformation using known reference points
        void Calibrate(List<CalibrationPoint> referencePoints);

        // Set the map dimensions for coordinate validation
        void SetMapBounds(double width, double height);
    }
}
