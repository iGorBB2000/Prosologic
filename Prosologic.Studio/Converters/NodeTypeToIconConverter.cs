using System.Globalization;
using System.Windows.Data;

namespace Prosologic.Studio.Converters;

[ValueConversion(typeof(object), typeof(string))]
public class NodeTypeToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Project" => "📁",
            "TagGroup" => "🗂",
            "Tag" => "🏷",
            _ => "📄"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
