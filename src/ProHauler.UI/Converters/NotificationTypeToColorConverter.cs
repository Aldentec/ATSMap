using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ProHauler.Core.Models;

namespace ProHauler.UI.Converters
{
    /// <summary>
    /// Converts a NotificationType to a color brush for display.
    /// </summary>
    public class NotificationTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NotificationType type)
            {
                return type switch
                {
                    NotificationType.Penalty => new SolidColorBrush(Color.FromRgb(231, 76, 60)),   // Red
                    NotificationType.Reward => new SolidColorBrush(Color.FromRgb(39, 174, 96)),    // Green
                    NotificationType.Neutral => new SolidColorBrush(Color.FromRgb(149, 165, 166)), // Gray
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
