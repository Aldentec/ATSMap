using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;  // For BitmapImage
using ProHauler.Core.Models;

namespace ProHauler.Core.Interfaces
{
    public interface IMapService
    {
        // Load map image asynchronously (like async function in JS)
        Task<BitmapImage> LoadMapAsync();

        // Get map metadata (dimensions, reference points)
        MapMetadata GetMetadata();
    }

    // Helper class for map metadata
    public class MapMetadata
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public List<CalibrationPoint> ReferencePoints { get; set; } = new();
    }
}
