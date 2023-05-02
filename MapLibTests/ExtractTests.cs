using FluentAssertions;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;

namespace MapLibTests;

public class ExtractTests : TestBase
{
    [ Theory ]
    [InlineData("BingMaps", 0, 0, 0, 256, 256, 1, 4)]
    [InlineData("BingMaps", 37, -122, 0, 20, 20, 1, 1)]
    [InlineData("BingMaps", 37, -122, 0, 40, 80, 4, 1)]
    [InlineData("BingMaps", 40, -122, 315, 40, 200, 4, 2)]
    [InlineData("BingMaps", 40, -112, 315, 100, 200, 4, 4)]
    [InlineData("OpenStreetMaps", 0, 0, 0, 256, 256, 1, 4)]
    [InlineData("OpenStreetMaps", 37, -122, 0, 20, 20, 1, 1)]
    [InlineData("OpenStreetMaps", 37, -122, 0, 40, 80, 4, 1)]
    [InlineData("OpenStreetMaps", 40, -122, 315, 40, 200, 4, 2)]
    [InlineData("OpenStreetMaps", 40, -112, 315, 100, 200, 4, 4)]
    [InlineData("OpenTopoMaps", 0, 0, 0, 256, 256, 1, 4)]
    [InlineData("OpenTopoMaps", 37, -122, 0, 20, 20, 1, 1)]
    [InlineData("OpenTopoMaps", 37, -122, 0, 40, 80, 4, 1)]
    [InlineData("OpenTopoMaps", 40, -122, 315, 40, 200, 4, 2)]
    [InlineData("OpenTopoMaps", 40, -112, 315, 100, 200, 4, 4)]
    [InlineData("GoogleMaps", 0, 0, 0, 256, 256, 1, 1)]
    [InlineData("GoogleMaps", 37, -122, 0, 20, 20, 1, 1)]
    [InlineData("GoogleMaps", 37, -122, 0, 40, 80, 4, 1)]
    [InlineData("GoogleMaps", 40, -122, 315, 40, 200, 4, 1)]
    [InlineData("GoogleMaps", 40, -112, 315, 100, 200, 4, 1)]
    public async Task BasicExtract(
        string projectionName,
        float latitude,
        float longitude,
        float heading,
        float height,
        float width,
        int scale,
        int numFragments
    )
    {
        var projection = CreateAndAuthenticateProjection( projectionName );
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();
        projection.MaxRequestLatency = 0;

        var region = new MapRegion( projection, LoggerFactory )
                    .Center( latitude, longitude )
                    .Heading( heading )
                    .Size( height, width )
                    .Scale( scale )
                    .Update();

        var result = await projection.LoadRegionAsync( region );
        result.Should().BeTrue();

        var numTiles = region.TilesHigh * region.TilesWide;
        numTiles.Should().Be( numFragments );
    }
}
