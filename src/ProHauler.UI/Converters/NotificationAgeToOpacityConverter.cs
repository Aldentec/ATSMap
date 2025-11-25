using System;
using System.Globalization;
using System.Windows.Data;

namespace ProHauler.UI.Converters
{
    /// <summary>
    /// Converts a notification timestamp to an opacity value for fade-out effect.
    /// Notifications fade out over 3 seconds.
    /// </summary>
    public class NotificationAgeToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime timestamp)
            {
                var age = (DateTime.Now - timestamp).TotalSeconds;

                // Fade out over 3 seconds
                if (age >= 3.0)
                {
                    return 0.0; // Fully transparent
                }
                else if (age >= 2.0)
                {
                    // Fade from 1.0 to 0.0 over the last second
                    return 1.0 - (age - 2.0);
                }
                else
                {
                    return 1.0; // Fully opaque
                }
            }

            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
