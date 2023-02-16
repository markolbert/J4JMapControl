using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using J4JMapLibrary;

namespace MapLibTests;

public class ExtractTests : TestBase
{
    [ Theory ]
    [InlineData("BingMaps", 0, 0, 0, 256, 256, 0, 1, 4)]
    [InlineData("BingMaps", 37, -122, 0, 20, 20, 0, 1, 1)]
    [InlineData("BingMaps", 37, -122, 0, 40, 80, 0, 4, 1)]
    [InlineData("BingMaps", 40, -122, 315, 40, 200, 0, 4, 2)]
    [InlineData("BingMaps", 40, -112, 315, 100, 200, 0, 4, 4)]
    [InlineData("OpenStreetMaps", 0, 0, 0, 256, 256, 0, 1, 4)]
    [InlineData("OpenStreetMaps", 37, -122, 0, 20, 20, 0, 1, 1)]
    [InlineData("OpenStreetMaps", 37, -122, 0, 40, 80, 0, 4, 1)]
    [InlineData("OpenStreetMaps", 40, -122, 315, 40, 200, 0, 4, 2)]
    [InlineData("OpenStreetMaps", 40, -112, 315, 100, 200, 0, 4, 4)]
    [InlineData("OpenTopoMaps", 0, 0, 0, 256, 256, 0, 1, 4)]
    [InlineData("OpenTopoMaps", 37, -122, 0, 20, 20, 0, 1, 1)]
    [InlineData("OpenTopoMaps", 37, -122, 0, 40, 80, 0, 4, 1)]
    [InlineData("OpenTopoMaps", 40, -122, 315, 40, 200, 0, 4, 2)]
    [InlineData("OpenTopoMaps", 40, -112, 315, 100, 200, 0, 4, 4)]
    [InlineData("GoogleMaps", 0, 0, 0, 256, 256, 0, 1, 1)]
    [InlineData("GoogleMaps", 37, -122, 0, 20, 20, 0, 1, 1)]
    [InlineData("GoogleMaps", 37, -122, 0, 40, 80, 0, 4, 1)]
    [InlineData("GoogleMaps", 40, -122, 315, 40, 200, 0, 4, 1)]
    [InlineData("GoogleMaps", 40, -112, 315, 100, 200, 0, 4, 1)]
    public async Task BasicExtract(
        string projectionName,
        float latitude,
        float longitude,
        float heading,
        float height,
        float width,
        float buffer,
        int scale,
        int numFragments
    )
    {
        var projBuilder = await GetFactory().CreateMapProjection( projectionName, null );
        projBuilder.Should().NotBeNull();
        projBuilder.Authenticated.Should().BeTrue();
        projBuilder.Projection.Should().NotBeNull();

        projBuilder.Projection!.MapServer.MaxRequestLatency = 0;

        var extractor = new SimpleMapFragments( projBuilder.Projection!, Logger );
        extractor.SetCenter( latitude, longitude );
        extractor.Heading = heading;
        extractor.SetHeightWidth( height, width );
        extractor.SetBuffer( buffer, buffer );
        extractor.Scale = scale;

        var fragments = await extractor.ToListAsync();
        fragments.Count.Should().Be( numFragments );
    }
}
