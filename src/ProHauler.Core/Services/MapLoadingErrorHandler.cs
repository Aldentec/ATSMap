using System;

namespace ProHauler.Core.Services;

/// <summary>
/// Handles errors during map loading with user-friendly messages
/// </summary>
public static class MapLoadingErrorHandler
{
    public static void LogError(Exception ex, string mapPath)
    {
        Console.WriteLine($"Map loading error for '{mapPath}': {ex.Message}");
    }

    public static Exception WrapException(Exception ex, string mapPath)
    {
        return new InvalidOperationException(
            $"Failed to load map from '{mapPath}': {ex.Message}",
            ex);
    }

    public static string GetErrorSummary(Exception ex)
    {
        return ex.Message;
    }
}
