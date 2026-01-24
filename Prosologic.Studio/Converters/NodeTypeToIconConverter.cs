using Avalonia.Data.Converters;
using Prosologic.Core.Enums;
using System;
using System.Globalization;

namespace Prosologic.Studio.Converters
{
    public class NodeTypeToIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is NodeType nodeType)
            {
                return nodeType switch
                {
                    NodeType.Project => "📁",
                    NodeType.TagGroup => "📂",
                    NodeType.Tag => "🏷️",
                    _ => "📄"
                };
            }
            return "📄";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
