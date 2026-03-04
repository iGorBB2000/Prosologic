using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Prosologic.Studio.Converters;

/// <summary>
/// Standard bool → Visibility. True = Visible, False = Collapsed
/// </summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility.Visible;
}
