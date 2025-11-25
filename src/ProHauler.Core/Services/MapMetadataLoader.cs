using System;
using System.IO;
using ProHauler.Core.Interfaces;

namespace ProHauler.Core.Services;

/// <summary>
/// Loads and validates map metadata
/// </summary>
public class MapMetadataLoader
{
    /// <summary>
    /// Validates that a map file exists and is readable
    /// </summary>
    public static void ValidateMapFile(string mapPath)
    {
        if (string.IsNullOrWhiteSpace(mapPath))
        {
            throw new ArgumentException("Map path cannot be empty", nameof(mapPath));
        }

        if (!File.Exists(mapPath))
        {
            throw new FileNotFoundException($"Map file not found: {mapPath}", mapPath);
        }
    }

    /// <summary>
    /// Enriches metadata with calibration points from configuration
    /// </summary>
    public void EnrichMetadata(MapMetadata metadata)
    {
        // TODO: Load calibration points from calibration.json
        // For now, this is a no-op
    }
}
