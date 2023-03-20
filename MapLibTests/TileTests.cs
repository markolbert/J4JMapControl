using FluentAssertions;
using J4JSoftware.J4JMapLibrary;

namespace MapLibTests;

public class TileTests : TestBase
{
    [ Theory ]
    [ InlineData( "BingMaps", 1, 0, 0, 128, 256, 0, 0, 0, 1, 1 ) ]
    [ InlineData( "OpenStreetMaps", 0, 0, 0, 128, 256, 0, 0, 0, 0, 0 ) ]
    [ InlineData( "BingMaps", 2, 37, -122, 128, 256, 0, 0, 1, 1, 1 ) ]
    [ InlineData( "BingMaps", 2, 37, -122, 128, 256, 45, 0, 1, 1, 2 ) ]
    [ InlineData( "BingMaps", 2, 37, -122, 128, 512, 45, 0, 0, 1, 2 ) ]
    [ InlineData( "BingMaps", 3, 37, -122, 512, 512, 45, 0, 1, 2, 4 ) ]
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

        var viewportData = projectionName switch
        {
            "GoogleMaps" => new NormalizedViewport( projection )
            {
                CenterLatitude = latitude,
                CenterLongitude = longitude,
                RequestedHeight = height,
                RequestedWidth = width,
                Scale = scale
            },

            _ => new Viewport( projection )
            {
                CenterLatitude = latitude,
                CenterLongitude = longitude,
                RequestedHeight = height,
                RequestedWidth = width,
                Heading = heading,
                Scale = scale
            }
        };

        var rawTiles = await projection.GetViewportAsync( viewportData ).ToListAsync();
        var tiles = rawTiles.Cast<ITiledFragment>().ToList();
        tiles.Count.Should().BeGreaterThan( 0 );

        tiles.Min( t => t.MapXTile ).Should().Be( minTileX );
        tiles.Min(t => t.MapYTile).Should().Be(minTileY);
        tiles.Max(t => t.MapXTile).Should().Be(maxTileX);
        tiles.Max(t => t.MapYTile).Should().Be(maxTileY);

        foreach ( var tile in tiles)
        {
            tile.ImageBytes.Should().BePositive();
        }
    }
}