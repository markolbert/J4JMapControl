using J4JMapLibrary;
using FluentAssertions;

namespace MapLibTests;

public class MiscTests
{

    [Theory]
    [InlineData("0", 1, 0, 0)]
    [InlineData("01", 2, 1, 0)]
    [InlineData("032", 3, 2, 3)]
    public async void DecodeQuadKeys(string quadKey, int scale, int xCoord, int yCoord)
    {
        var components = MapExtensions.ToTileCoordinates(quadKey);

        components.Scale.Should().Be(scale);
        components.XTile.Should().Be(xCoord);
        components.YTile.Should().Be(yCoord);
    }
}
