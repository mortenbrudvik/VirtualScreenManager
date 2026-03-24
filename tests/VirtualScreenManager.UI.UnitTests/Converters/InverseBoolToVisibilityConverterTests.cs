using System.Globalization;
using System.Windows;
using Shouldly;
using VirtualScreenManager.UI.Converters;
using Xunit;

namespace VirtualScreenManager.UI.UnitTests.Converters;

public class InverseBoolToVisibilityConverterTests
{
    private readonly InverseBoolToVisibilityConverter _sut = new();

    [Fact]
    public void Convert_True_ReturnsCollapsed()
    {
        _sut.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture)
            .ShouldBe(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_False_ReturnsVisible()
    {
        _sut.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture)
            .ShouldBe(Visibility.Visible);
    }

    [Fact]
    public void Convert_NonBool_ReturnsVisible()
    {
        _sut.Convert("not a bool", typeof(Visibility), null!, CultureInfo.InvariantCulture)
            .ShouldBe(Visibility.Visible);
    }

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
    {
        Should.Throw<NotSupportedException>(() =>
            _sut.ConvertBack(Visibility.Collapsed, typeof(bool), null!, CultureInfo.InvariantCulture));
    }
}
