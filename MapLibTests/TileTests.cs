using FluentAssertions;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;

namespace MapLibTests;

public class TileTests : TestBase
{
    [ Theory ]
    [ InlineData( "BingMaps", 1, 0, 0, 128, 256, 0, 0, 0, 1, 1 ) ]
    [ InlineData( "OpenStreetMaps", 0, 0, 0, 128, 256, 0, 0, 0, 0, 0 ) ]
    [ InlineData( "BingMaps", 2, 37, -122, 128, 256, 0, 0, 1, 1, 1 ) ]
    [ InlineData( "BingMaps", 2, 37, -122, 128, 256, 45, 0, 1, 1, 2 ) ]
    [ InlineData( "BingMaps", 2, 37, -122, 128, 512, 45, 0, 0, 3, 2 ) ]
    [ InlineData( "BingMaps", 3, 37, -122, 512, 512, 45, 0, 1, 7, 4 ) ]
    [ InlineData( "BingMaps", 3, 37, -122, 512, 512, 75, 0, 1, 2, 4 ) ]
    [ InlineData( "BingMaps", 3, 37, 97, 512, 512, 75, 4, 1, 7, 4 ) ]
    [ InlineData( "BingMaps", 3, 37, 97, 512, 512, 0, 5, 2, 7, 4 ) ]
    public async Task TileRegion(
        string projectionName,
        int scale,
        float latitude,
        float longitude,
        int height,
        int width,
        float heading,
        int minTileX,
        int minTileY,
        int maxTileX,
        int maxTileY
    )
    {
        var projection = await CreateProjection( projectionName );
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var region = new MapRegion( projection, Logger )
                    .Center( latitude, longitude )
                    .Size( height, width )
                    .Heading( heading )
                    .Scale( scale )
                    .Build();

        var result = await projection.LoadRegionAsync( region );
        result.Should().BeTrue();
        region.MapTiles.Count.Should().BeGreaterThan( 0 );

        region.MapTiles.Min( t => t.RetrievedX ).Should().Be( minTileX );
        region.MapTiles.Min( t => t.RetrievedY ).Should().Be( minTileY );
        region.MapTiles.Max( t => t.RetrievedX ).Should().Be( maxTileX );
        region.MapTiles.Max( t => t.RetrievedY ).Should().Be( maxTileY );

        foreach( var tile in region.MapTiles )
        {
            tile.ImageBytes.Should().BePositive();
        }
    }
}