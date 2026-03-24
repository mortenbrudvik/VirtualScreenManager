using System.Globalization;
using System.Windows.Data;

namespace VirtualScreenManager.UI.Converters;

public class VirtualLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? "Virtual" : "Physical";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
