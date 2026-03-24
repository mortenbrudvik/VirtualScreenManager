using System.Globalization;
using Shouldly;
using VirtualScreenManager.UI.Converters;
using Xunit;

namespace VirtualScreenManager.UI.UnitTests.Converters;

public class InverseBoolConverterTests
{
    private readonly InverseBoolConverter _sut = new();

    [Fact]
    public void Convert_True_ReturnsFalse()
    {
        _sut.Convert(true, typeof(bool), null!, CultureInfo.InvariantCulture)
            .ShouldBe(false);
    }

    [Fact]
    public void Convert_False_ReturnsTrue()
    {
        _sut.Convert(false, typeof(bool), null!, CultureInfo.InvariantCulture)
            .ShouldBe(true);
    }

    [Fact]
    public void Convert_NonBool_ReturnsFalse()
    {
        _sut.Convert("not a bool", typeof(bool), null!, CultureInfo.InvariantCulture)
            .ShouldBe(false);
    }

    [Fact]
    public void ConvertBack_True_ReturnsFalse()
    {
        _sut.ConvertBack(true, typeof(bool), null!, CultureInfo.InvariantCulture)
            .ShouldBe(false);
    }

    [Fact]
    public void ConvertBack_False_ReturnsTrue()
    {
        _sut.ConvertBack(false, typeof(bool), null!, CultureInfo.InvariantCulture)
            .ShouldBe(true);
    }
}
