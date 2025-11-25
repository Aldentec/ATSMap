namespace ProHauler.Core.Helpers;

/// <summary>
/// Helper class for common score-related operations.
/// Provides utilities for score validation, grade conversion, and description generation.
/// </summary>
public static class ScoreHelper
{
    /// <summary>
    /// Clamps a score value to the valid range of 0-100.
    /// </summary>
    /// <param name="score">The score value to clamp.</param>
    /// <returns>The clamped score value between 0 and 100.</returns>
    public static float ClampScore(float score)
    {
        return Math.Max(0, Math.Min(100, score));
    }

    /// <summary>
    /// Converts a numeric score (0-100) to a letter grade.
    /// </summary>
    /// <param name="score">The numeric score to convert.</param>
    /// <returns>A letter grade string (A+, A, B+, B, C+, C, D+, D, or F).</returns>
    public static string ConvertScoreToGrade(float score)
    {
        return score switch
        {
            >= 95.0f => "A+",
            >= 90.0f => "A",
            >= 85.0f => "B+",
            >= 80.0f => "B",
            >= 75.0f => "C+",
            >= 70.0f => "C",
            >= 65.0f => "D+",
            >= 60.0f => "D",
            _ => "F"
        };
    }

    /// <summary>
    /// Converts a letter grade to a numeric score for visualization purposes.
    /// Uses the midpoint of each grade range.
    /// </summary>
    /// <param name="grade">The letter grade to convert.</param>
    /// <returns>The numeric score (0-100) representing the grade.</returns>
    public static float ConvertGradeToScore(string grade)
    {
        return grade switch
        {
            "A+" => 97.5f,
            "A" => 92.5f,
            "B+" => 87.5f,
            "B" => 82.5f,
            "C+" => 77.5f,
            "C" => 72.5f,
            "D+" => 67.5f,
            "D" => 62.5f,
            "F" => 50.0f,
            _ => 0.0f
        };
    }

    /// <summary>
    /// Gets a description for a smoothness score based on its value.
    /// </summary>
    /// <param name="score">The smoothness score (0-100).</param>
    /// <returns>A descriptive string explaining the score level.</returns>
    public static string GetSmoothnessDescription(float score)
    {
        if (score >= 90.0f) return "Excellent smooth driving";
        if (score >= 75.0f) return "Good acceleration and braking";
        if (score >= 60.0f) return "Some harsh inputs detected";
        return "Frequent harsh acceleration/braking";
    }

    /// <summary>
    /// Gets a description for a speed compliance score based on its value.
    /// </summary>
    /// <param name="score">The speed compliance score (0-100).</param>
    /// <returns>A descriptive string explaining the score level.</returns>
    public static string GetSpeedComplianceDescription(float score)
    {
        if (score >= 95.0f) return "Excellent speed limit adherence";
        if (score >= 80.0f) return "Good speed control";
        if (score >= 60.0f) return "Occasional speeding detected";
        return "Frequent speed limit violations";
    }

    /// <summary>
    /// Gets a description for a safety score based on its value.
    /// </summary>
    /// <param name="score">The safety score (0-100).</param>
    /// <returns>A descriptive string explaining the score level.</returns>
    public static string GetSafetyDescription(float score)
    {
        if (score >= 90.0f) return "Excellent safety practices";
        if (score >= 75.0f) return "Good safety awareness";
        if (score >= 60.0f) return "Some safety issues detected";
        return "Multiple safety violations";
    }

    /// <summary>
    /// Gets a color indicator string based on the score value.
    /// </summary>
    /// <param name="score">The score value (0-100).</param>
    /// <returns>A color indicator string ("Green", "Gray", or "Red").</returns>
    public static string GetColorIndicator(float score)
    {
        if (score >= 80.0f) return "Green";
        if (score >= 60.0f) return "Gray";
        return "Red";
    }

    /// <summary>
    /// Gets a description for vehicle condition based on damage percentage and streak.
    /// </summary>
    /// <param name="damagePercent">The vehicle damage percentage (0-100).</param>
    /// <param name="streakMinutes">The damage-free streak in minutes.</param>
    /// <returns>A descriptive string explaining the vehicle condition.</returns>
    public static string GetVehicleConditionDescription(float damagePercent, float streakMinutes)
    {
        if (damagePercent <= 5.0f)
        {
            if (streakMinutes >= 30.0f)
                return $"Excellent condition ({damagePercent:F1}% damage, {streakMinutes:F0} min streak)";
            return $"Excellent condition ({damagePercent:F1}% damage)";
        }
        if (damagePercent <= 15.0f)
        {
            if (streakMinutes >= 15.0f)
                return $"Good condition ({damagePercent:F1}% damage, {streakMinutes:F0} min streak)";
            return $"Minor damage ({damagePercent:F1}%)";
        }
        if (damagePercent <= 30.0f)
        {
            return $"Moderate damage ({damagePercent:F1}%)";
        }
        if (damagePercent <= 50.0f)
        {
            return $"Significant damage ({damagePercent:F1}%)";
        }
        return $"Severe damage ({damagePercent:F1}%)";
    }
}
