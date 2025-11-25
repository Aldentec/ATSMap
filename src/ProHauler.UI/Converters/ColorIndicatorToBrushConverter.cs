using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProHauler.UI.Converters
{
    /// <summary>
    /// Converts a color indicator string (Green, Red, Gray) to a SolidColorBrush.
    /// </summary>
    public class ColorIndicatorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorIndicator)
            {
                return colorIndicator switch
                {
                    "Green" => new SolidColorBrush(Color.FromRgb(39, 174, 96)),  // #27AE60
                    "Red" => new SolidColorBrush(Color.FromRgb(231, 76, 60)),    // #E74C3C
                    "Gray" => new SolidColorBrush(Color.FromRgb(149, 165, 166)), // #95A5A6
                    _ => new SolidColorBrush(Color.FromRgb(149, 165, 166))       // Default to gray
                };
            }

            return new SolidColorBrush(Color.FromRgb(149, 165, 166));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
