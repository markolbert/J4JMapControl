using J4JMapLibrary;
using FluentAssertions;

namespace MapLibTests;

public class MiscTests
{

    [Theory]
    [InlineData("0", 1, 0, 0)]
    [InlineData("01", 2, 1, 0)]
    [InlineData("032", 3, 2, 3)]
    public void DecodeQuadKeys(string quadKey, int scale, int x, int y)
    {
        var (calcScale, xTile, yTile) = MapExtensions.ToTileCoordinates(quadKey);

        calcScale.Should().Be(scale);
        xTile.Should().Be(x);
        yTile.Should().Be(y);
    }
}
