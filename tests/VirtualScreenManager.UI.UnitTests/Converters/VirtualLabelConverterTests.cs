using System.Globalization;
using Shouldly;
using VirtualScreenManager.UI.Converters;
using Xunit;

namespace VirtualScreenManager.UI.UnitTests.Converters;

public class VirtualLabelConverterTests
{
    private readonly VirtualLabelConverter _sut = new();

    [Fact]
    public void Convert_True_ReturnsVirtual()
    {
        _sut.Convert(true, typeof(string), null!, CultureInfo.InvariantCulture)
            .ShouldBe("Virtual");
    }

    [Fact]
    public void Convert_False_ReturnsPhysical()
    {
        _sut.Convert(false, typeof(string), null!, CultureInfo.InvariantCulture)
            .ShouldBe("Physical");
    }

    [Fact]
    public void Convert_NonBool_ReturnsPhysical()
    {
        _sut.Convert("not a bool", typeof(string), null!, CultureInfo.InvariantCulture)
            .ShouldBe("Physical");
    }

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
    {
        Should.Throw<NotSupportedException>(() =>
            _sut.ConvertBack("Virtual", typeof(bool), null!, CultureInfo.InvariantCulture));
    }
}
