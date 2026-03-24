using System.Globalization;
using System.Windows.Data;
using Microsoft.Extensions.Logging;
using Wpf.Ui.Controls;

namespace VirtualScreenManager.UI.Converters;

public class LogLevelToSeverityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is LogLevel level
            ? level switch
            {
                LogLevel.Error or LogLevel.Critical => InfoBarSeverity.Error,
                LogLevel.Warning => InfoBarSeverity.Warning,
                _ => InfoBarSeverity.Informational
            }
            : InfoBarSeverity.Informational;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
