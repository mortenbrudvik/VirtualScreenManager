using System.Globalization;
using Microsoft.Extensions.Logging;
using Shouldly;
using VirtualScreenManager.UI.Converters;
using Wpf.Ui.Controls;
using Xunit;

namespace VirtualScreenManager.UI.UnitTests.Converters;

public class LogLevelToSeverityConverterTests
{
    private readonly LogLevelToSeverityConverter _sut = new();

    [Fact]
    public void Convert_Error_ReturnsError()
    {
        _sut.Convert(LogLevel.Error, typeof(InfoBarSeverity), null!, CultureInfo.InvariantCulture)
            .ShouldBe(InfoBarSeverity.Error);
    }

    [Fact]
    public void Convert_Critical_ReturnsError()
    {
        _sut.Convert(LogLevel.Critical, typeof(InfoBarSeverity), null!, CultureInfo.InvariantCulture)
            .ShouldBe(InfoBarSeverity.Error);
    }

    [Fact]
    public void Convert_Warning_ReturnsWarning()
    {
        _sut.Convert(LogLevel.Warning, typeof(InfoBarSeverity), null!, CultureInfo.InvariantCulture)
            .ShouldBe(InfoBarSeverity.Warning);
    }

    [Fact]
    public void Convert_Information_ReturnsInformational()
    {
        _sut.Convert(LogLevel.Information, typeof(InfoBarSeverity), null!, CultureInfo.InvariantCulture)
            .ShouldBe(InfoBarSeverity.Informational);
    }

    [Fact]
    public void Convert_Trace_ReturnsInformational()
    {
        _sut.Convert(LogLevel.Trace, typeof(InfoBarSeverity), null!, CultureInfo.InvariantCulture)
            .ShouldBe(InfoBarSeverity.Informational);
    }

    [Fact]
    public void Convert_NonLogLevel_ReturnsInformational()
    {
        _sut.Convert("not a log level", typeof(InfoBarSeverity), null!, CultureInfo.InvariantCulture)
            .ShouldBe(InfoBarSeverity.Informational);
    }

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
    {
        Should.Throw<NotSupportedException>(() =>
            _sut.ConvertBack(InfoBarSeverity.Error, typeof(LogLevel), null!, CultureInfo.InvariantCulture));
    }
}
