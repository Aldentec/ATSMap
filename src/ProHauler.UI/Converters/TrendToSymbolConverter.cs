using System.Globalization;
using System.Windows.Data;
using ProHauler.Core.Models;

namespace ProHauler.UI.Converters;

/// <summary>
/// Converts TrendIndicator enum values to arrow symbols for visual display.
/// Maps Up to ↑, Down to ↓, and Stable to →.
/// </summary>
public class TrendToSymbolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TrendIndicator trend)
        {
            return trend switch
            {
                TrendIndicator.Up => "↑",
                TrendIndicator.Down => "↓",
                TrendIndicator.Stable => "→",
                _ => "→"
            };
        }

        return "→";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
