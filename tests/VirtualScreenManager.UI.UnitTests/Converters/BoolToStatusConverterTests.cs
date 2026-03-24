using System.Globalization;
using Shouldly;
using VirtualScreenManager.UI.Converters;
using Xunit;

namespace VirtualScreenManager.UI.UnitTests.Converters;

public class BoolToStatusConverterTests
{
    private readonly BoolToStatusConverter _sut = new();

    [Fact]
    public void Convert_True_ReturnsOnline()
    {
        _sut.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture)
            .ShouldBe("Online");
    }

    [Fact]
    public void Convert_False_ReturnsOffline()
    {
        _sut.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture)
            .ShouldBe("Offline");
    }

    [Fact]
    public void Convert_NonBool_ReturnsOffline()
    {
        _sut.Convert("not a bool", typeof(string), null!, CultureInfo.InvariantCulture)
            .ShouldBe("Offline");
    }

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
    {
        Should.Throw<NotSupportedException>(() =>
            _sut.ConvertBack("Online", typeof(bool), null!, CultureInfo.InvariantCulture));
    }
}
