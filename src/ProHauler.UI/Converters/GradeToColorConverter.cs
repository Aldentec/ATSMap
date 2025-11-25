using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ProHauler.UI.Helpers;

namespace ProHauler.UI.Converters;

/// <summary>
/// Converts letter grades (A+ to F) to color brushes for visual feedback.
/// Maps grades to colors: A+/A = green, B+/B = blue, C = yellow, D/F = red.
/// </summary>
public class GradeToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string grade)
        {
            return ColorHelper.GetBrushForGrade(grade);
        }

        return ColorHelper.GrayBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
