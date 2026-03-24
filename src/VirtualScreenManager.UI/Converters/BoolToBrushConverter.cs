using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace VirtualScreenManager.UI.Converters;

public class BoolToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush FallbackSuccess = CreateFrozen(Color.FromRgb(0x2D, 0xC6, 0x53));
    private static readonly SolidColorBrush FallbackCritical = CreateFrozen(Color.FromRgb(0xE8, 0x48, 0x55));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var resourceKey = value is true
            ? "SystemFillColorSuccessBrush"
            : "SystemFillColorCriticalBrush";

        var fallback = value is true ? FallbackSuccess : FallbackCritical;

        return Application.Current?.TryFindResource(resourceKey) as Brush ?? fallback;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static SolidColorBrush CreateFrozen(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
