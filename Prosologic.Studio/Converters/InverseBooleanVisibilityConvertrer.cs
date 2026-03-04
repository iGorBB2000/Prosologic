using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Prosologic.Studio.Converters;

/// <summary>
/// Inverse: False = Visible, True = Collapsed
/// </summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is not Visibility.Visible;
}
