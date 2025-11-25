using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ProHauler.Core.Interfaces;
using ProHauler.Core.Models;

namespace ProHauler.Core.Services
{
    /// <summary>
    /// Map service that loads a single large PNG image for the entire map.
    /// Implements caching and thread-safe bitmap handling.
    /// </summary>
    public class SingleImageMapService : IMapService
    {
        private readonly AppConfiguration _configuration;
        private readonly MapMetadataLoader _metadataLoader;
        private BitmapImage? _cachedMap;
        private MapMetadata? _metadata;
        private readonly object _lock = new object();

        public SingleImageMapService(AppConfiguration configuration, MapMetadataLoader? metadataLoader = null)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _metadataLoader = metadataLoader ?? new MapMetadataLoader();
        }

        /// <summary>
        /// Loads the map image asynchronously with caching.
        /// Uses BitmapImage with CacheOption.OnLoad for efficient loading.
        /// Freezes the bitmap to make it thread-safe and immutable.
        /// </summary>
        public async Task<BitmapImage> LoadMapAsync()
        {
            // Return cached map if already loaded
            if (_cachedMap != null)
            {
                return _cachedMap;
            }

            // Load on background thread to avoid blocking UI
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    // Double-check after acquiring lock
                    if (_cachedMap != null)
                    {
                        return _cachedMap;
                    }

                    string mapPath = _configuration.MapImagePath;

                    try
                    {
                        // Validate file exists and is readable
                        MapMetadataLoader.ValidateMapFile(mapPath);

                        // Create and configure BitmapImage
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();

                        // Use absolute URI for file path
                        bitmap.UriSource = new Uri(Path.GetFullPath(mapPath), UriKind.Absolute);

                        // CacheOption.OnLoad loads the entire image into memory immediately
                        // This avoids keeping the file locked and improves performance
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;

                        bitmap.EndInit();

                        // Freeze the bitmap to make it thread-safe and immutable
                        // This allows it to be used across threads without synchronization
                        bitmap.Freeze();

                        // Cache the loaded bitmap
                        _cachedMap = bitmap;

                        // Initialize metadata from loaded image
                        _metadata = new MapMetadata
                        {
                            Width = bitmap.PixelWidth,
                            Height = bitmap.PixelHeight,
                            ReferencePoints = new System.Collections.Generic.List<CalibrationPoint>()
                        };

                        // Load calibration points from configuration
                        try
                        {
                            _metadataLoader.EnrichMetadata(_metadata);
                        }
                        catch (Exception ex)
                        {
                            // Log warning but don't fail - calibration can be loaded later
                            Console.WriteLine($"Warning: Failed to load calibration points: {ex.Message}");
                        }

                        return _cachedMap;
                    }
                    catch (Exception ex)
                    {
                        // Log the error with detailed information
                        MapLoadingErrorHandler.LogError(ex, mapPath);

                        // Wrap with user-friendly message and re-throw
                        throw MapLoadingErrorHandler.WrapException(ex, mapPath);
                    }
                }
            });
        }

        /// <summary>
        /// Gets map metadata including dimensions and reference points.
        /// Loads the map if not already loaded to obtain dimensions.
        /// </summary>
        public MapMetadata GetMetadata()
        {
            // If metadata is already initialized, return it
            if (_metadata != null)
            {
                return _metadata;
            }

            // If map is loaded but metadata isn't initialized (shouldn't happen)
            if (_cachedMap != null)
            {
                _metadata = new MapMetadata
                {
                    Width = _cachedMap.PixelWidth,
                    Height = _cachedMap.PixelHeight,
                    ReferencePoints = new System.Collections.Generic.List<CalibrationPoint>()
                };

                // Load calibration points
                try
                {
                    _metadataLoader.EnrichMetadata(_metadata);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to load calibration points: {ex.Message}");
                }

                return _metadata;
            }

            // Map not loaded yet - need to load it to get dimensions
            // This is a synchronous call, so we'll do a simple load
            string mapPath = _configuration.MapImagePath;

            try
            {
                // Validate file exists and is readable
                MapMetadataLoader.ValidateMapFile(mapPath);

                // Load synchronously to get dimensions
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(Path.GetFullPath(mapPath), UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                _cachedMap = bitmap;
                _metadata = new MapMetadata
                {
                    Width = bitmap.PixelWidth,
                    Height = bitmap.PixelHeight,
                    ReferencePoints = new System.Collections.Generic.List<CalibrationPoint>()
                };

                // Load calibration points
                try
                {
                    _metadataLoader.EnrichMetadata(_metadata);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to load calibration points: {ex.Message}");
                }

                return _metadata;
            }
            catch (Exception ex)
            {
                // Log the error with detailed information
                MapLoadingErrorHandler.LogError(ex, mapPath);

                // Wrap with user-friendly message and re-throw
                throw MapLoadingErrorHandler.WrapException(ex, mapPath);
            }
        }

        /// <summary>
        /// Clears the cached map image to free memory.
        /// Useful if the map path changes or to force a reload.
        /// </summary>
        public void ClearCache()
        {
            lock (_lock)
            {
                _cachedMap = null;
                _metadata = null;
            }
        }

        /// <summary>
        /// Checks if a valid map is currently loaded.
        /// </summary>
        public bool IsMapLoaded()
        {
            return _cachedMap != null;
        }

        /// <summary>
        /// Attempts to load the map and returns success status.
        /// Does not throw exceptions - returns false on failure.
        /// </summary>
        public async Task<bool> TryLoadMapAsync()
        {
            try
            {
                await LoadMapAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Error already logged by LoadMapAsync
                Console.WriteLine($"Map loading failed: {MapLoadingErrorHandler.GetErrorSummary(ex)}");
                return false;
            }
        }
    }
}
