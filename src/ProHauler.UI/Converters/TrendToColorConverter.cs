using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ProHauler.Core.Models;
using ProHauler.UI.Helpers;

namespace ProHauler.UI.Converters;

/// <summary>
/// Converts TrendIndicator enum values to color brushes for visual feedback.
/// Maps Up to green, Down to red, and Stable to gray.
/// </summary>
public class TrendToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TrendIndicator trend)
        {
            return ColorHelper.GetBrushForTrend(trend);
        }

        return ColorHelper.GrayBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
