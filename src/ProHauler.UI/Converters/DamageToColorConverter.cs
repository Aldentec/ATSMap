using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ProHauler.UI.Helpers;

namespace ProHauler.UI.Converters
{
    /// <summary>
    /// Converts a damage percentage (0-100) to a color.
    /// Lower damage = green, higher damage = red.
    /// </summary>
    public class DamageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float damage)
            {
                return ColorHelper.GetBrushForDamage(damage);
            }

            return ColorHelper.GrayBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
