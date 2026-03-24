using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace VirtualScreenManager.UI.Converters;

public class BoolToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true
            ? new SolidColorBrush(Color.FromRgb(0x2D, 0xC6, 0x53)) // Green
            : new SolidColorBrush(Color.FromRgb(0xE8, 0x48, 0x55)); // Red
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
