using System.Globalization;
using System.Windows.Data;

namespace ProHauler.UI.Converters;

/// <summary>
/// Converts speed from m/s to MPH
/// </summary>
public class SpeedToMphConverter : IValueConverter
{
    private const double MetersPerSecondToMph = 2.23694;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is float speed)
        {
            double mph = speed * MetersPerSecondToMph;
            return $"{mph:F0}";
        }
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
