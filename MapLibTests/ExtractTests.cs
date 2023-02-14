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
    [InlineData("BingMaps", 0,0,0,256,256,0,1,4)]
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

        var extractor = new MapExtractNg( projBuilder.Projection!, Logger );
        extractor.SetCenter( latitude, longitude );
        extractor.Heading = heading;
        extractor.SetHeightWidth( height, width );
        extractor.SetBuffer( buffer, buffer );
        extractor.Scale = scale;

        var fragments = await extractor.ToListAsync();
        fragments.Count.Should().Be( numFragments );
    }
}
