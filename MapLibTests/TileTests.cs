using FluentAssertions;
using J4JMapLibrary;

namespace MapLibTests;

public class TileTests : TestBase
{
    [ Theory ]
    [ InlineData( "BingMaps", 1, 0, 0, 128, 256, 0, 0, 0, 1, 1 ) ]
    [ InlineData( "OpenStreetMaps", 0, 0, 0, 128, 256, 0, 0, 0, 0, 0 ) ]
    [ InlineData( "BingMaps", 2, 37, -122, 128, 256, 0, 0, 2, 1, 2 ) ]
    [ InlineData( "BingMaps", 2, 37, -122, 128, 256, 45, 0, 1, 1, 2 ) ]
    [ InlineData( "BingMaps", 2, 37, -122, 128, 512, 45, 0, 1, 1, 3 ) ]
    [ InlineData( "BingMaps", 3, 37, -122, 512, 512, 45, 0, 3, 2, 6 ) ]
    [ InlineData( "BingMaps", 3, 37, -122, 512, 512, 75, 0, 3, 2, 6 ) ]
    [ InlineData( "BingMaps", 3, 37, 97, 512, 512, 75, 4, 3, 7, 6 ) ]
    [ InlineData( "BingMaps", 3, 37, 97, 512, 512, 0, 5, 3, 7, 5 ) ]
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
        var result = await GetFactory().CreateMapProjection( projectionName, null );
        var projection = result.Projection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.MapScale.Scale = scale;

        var viewportData = projectionName switch
        {
            "GoogleMaps" => new NormalizedViewport( projection )
            {
                CenterLatitude = latitude, CenterLongitude = longitude, Height = height, Width = width
            },

            _ => new Viewport( projection )
            {
                CenterLatitude = latitude,
                CenterLongitude = longitude,
                Height = height,
                Width = width,
                Heading = heading
            }
        };

        var extract = projectionName switch
        {
            "GoogleMaps" => await ( (IStaticProjection) projection ).GetExtractAsync( (IViewport) viewportData ) as IMapExtract,
            _ => await ( (ITiledProjection) projection ).GetExtractAsync( (IViewport) viewportData )
        };

        extract.Should().NotBeNull();

        object? bounds;

        if( projectionName == "GoogleMaps" )
        {
            ( (StaticExtract) extract! ).TryGetBounds( out var staticBounds ).Should().BeTrue();
            bounds = staticBounds;
        }
        else
        {
            ((TiledExtract)extract!).TryGetBounds(out var tiledBounds).Should().BeTrue();
            bounds = tiledBounds;
        }

        bounds.Should().NotBeNull();

        var testBounds = new TiledBounds( new TileCoordinates( minTileX, minTileY ),
                                         new TileCoordinates( maxTileX, maxTileY ) );

        bounds!.Should().Be( testBounds );

        await foreach( var tile in extract.GetTilesAsync(scale) )
        {
            tile.ImageBytes.Should().BeNegative();
        }
    }
}