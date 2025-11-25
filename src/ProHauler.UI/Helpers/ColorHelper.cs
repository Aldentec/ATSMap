using System.Windows.Media;

namespace ProHauler.UI.Helpers;

/// <summary>
/// Helper class for common color operations in the UI.
/// Provides centralized color definitions and conversion utilities.
/// </summary>
public static class ColorHelper
{
    // Standard color palette for performance indicators
    private static readonly Color GreenColor = Color.FromRgb(39, 174, 96);    // #27AE60
    private static readonly Color BlueColor = Color.FromRgb(52, 152, 219);    // #3498DB
    private static readonly Color YellowColor = Color.FromRgb(243, 156, 18);  // #F39C12
    private static readonly Color OrangeColor = Color.FromRgb(230, 126, 34);  // #E67E22
    private static readonly Color RedColor = Color.FromRgb(231, 76, 60);      // #E74C3C
    private static readonly Color GrayColor = Color.FromRgb(149, 165, 166);   // #95A5A6

    /// <summary>
    /// Gets a SolidColorBrush for the green performance indicator.
    /// Used for excellent/good performance (A grades, high scores).
    /// </summary>
    public static SolidColorBrush GreenBrush => new SolidColorBrush(GreenColor);

    /// <summary>
    /// Gets a SolidColorBrush for the blue performance indicator.
    /// Used for above-average performance (B grades).
    /// </summary>
    public static SolidColorBrush BlueBrush => new SolidColorBrush(BlueColor);

    /// <summary>
    /// Gets a SolidColorBrush for the yellow performance indicator.
    /// Used for average performance (C grades).
    /// </summary>
    public static SolidColorBrush YellowBrush => new SolidColorBrush(YellowColor);

    /// <summary>
    /// Gets a SolidColorBrush for the orange performance indicator.
    /// Used for warning states (moderate damage).
    /// </summary>
    public static SolidColorBrush OrangeBrush => new SolidColorBrush(OrangeColor);

    /// <summary>
    /// Gets a SolidColorBrush for the red performance indicator.
    /// Used for poor performance (D/F grades, low scores).
    /// </summary>
    public static SolidColorBrush RedBrush => new SolidColorBrush(RedColor);

    /// <summary>
    /// Gets a SolidColorBrush for the gray performance indicator.
    /// Used for neutral/unknown states.
    /// </summary>
    public static SolidColorBrush GrayBrush => new SolidColorBrush(GrayColor);

    /// <summary>
    /// Converts a numeric score (0-100) to an appropriate color brush.
    /// Maps scores to colors based on grade thresholds.
    /// </summary>
    /// <param name="score">The numeric score to convert.</param>
    /// <returns>A SolidColorBrush representing the score level.</returns>
    public static SolidColorBrush GetBrushForScore(float score)
    {
        return score switch
        {
            >= 90.0f => GreenBrush,
            >= 80.0f => BlueBrush,
            >= 70.0f => YellowBrush,
            >= 0.0f => RedBrush,
            _ => GrayBrush
        };
    }

    /// <summary>
    /// Converts a letter grade to an appropriate color brush.
    /// </summary>
    /// <param name="grade">The letter grade (A+, A, B+, etc.).</param>
    /// <returns>A SolidColorBrush representing the grade level.</returns>
    public static SolidColorBrush GetBrushForGrade(string grade)
    {
        return grade switch
        {
            "A+" or "A" => GreenBrush,
            "B+" or "B" => BlueBrush,
            "C+" or "C" => YellowBrush,
            "D+" or "D" or "F" => RedBrush,
            _ => GrayBrush
        };
    }

    /// <summary>
    /// Converts a damage percentage to an appropriate color brush.
    /// Lower damage = green, higher damage = red.
    /// </summary>
    /// <param name="damagePercent">The damage percentage (0-100).</param>
    /// <returns>A SolidColorBrush representing the damage level.</returns>
    public static SolidColorBrush GetBrushForDamage(float damagePercent)
    {
        if (damagePercent <= 5.0f) return GreenBrush;
        if (damagePercent <= 15.0f) return YellowBrush;
        if (damagePercent <= 30.0f) return OrangeBrush;
        return RedBrush;
    }

    /// <summary>
    /// Converts a trend indicator to an appropriate color brush.
    /// </summary>
    /// <param name="trend">The trend indicator (Up, Down, Stable).</param>
    /// <returns>A SolidColorBrush representing the trend.</returns>
    public static SolidColorBrush GetBrushForTrend(Core.Models.TrendIndicator trend)
    {
        return trend switch
        {
            Core.Models.TrendIndicator.Up => GreenBrush,
            Core.Models.TrendIndicator.Down => RedBrush,
            _ => GrayBrush
        };
    }
}
