using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProHauler.UI.Converters;

/// <summary>
/// Converts connection status string to color brush
/// </summary>
public class ConnectionStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            if (status.Contains("Connected", StringComparison.OrdinalIgnoreCase) &&
                !status.Contains("Disconnected", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Color.FromRgb(39, 174, 96)); // Green #27AE60
            }
            else if (status.Contains("Connecting", StringComparison.OrdinalIgnoreCase) ||
                     status.Contains("Waiting", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Color.FromRgb(243, 156, 18)); // Orange #F39C12
            }
            else if (status.Contains("Error", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Red #E74C3C
            }
        }

        // Default: Disconnected (Red)
        return new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Red #E74C3C
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
