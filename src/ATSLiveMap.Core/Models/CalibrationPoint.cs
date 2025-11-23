using System.Windows;

namespace ATSLiveMap.Core.Models
{
    public class CalibrationPoint
    {
        public string LocationName { get; set; } = string.Empty;
        public Vector3 WorldPosition { get; set; }      // ATS game coordinates
        public Point MapPixelPosition { get; set; }     // Pixel position on map image
    }
}
