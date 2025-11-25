using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ProHauler.UI.Helpers;

namespace ProHauler.UI.Converters;

/// <summary>
/// Converts numeric scores (0-100) to color brushes for visual feedback.
/// Maps scores to colors based on grade thresholds: A = green, B = blue, C = yellow, D/F = red.
/// </summary>
public class ScoreToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is float score || value is double)
        {
            var scoreValue = System.Convert.ToSingle(value);
            return ColorHelper.GetBrushForScore(scoreValue);
        }

        return ColorHelper.GrayBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
