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
        var projection = CreateAndAuthenticateProjection( projectionName );
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var region = new MapRegion( projection, LoggerFactory )
                    .Center( latitude, longitude )
                    .Size( height, width )
                    .Heading( heading )
                    .Scale( scale )
                    .Update();

        var result = await projection.LoadRegionAsync( region );
        result.Should().BeTrue();

        var numTiles = region.TilesHigh * region.TilesWide;
        numTiles.Should().BeGreaterThan( 0 );

        region.Min( t => t.X ).Should().Be( minTileX );
        region.Min( t => t.Y ).Should().Be( minTileY );
        region.Max( t => t.X ).Should().Be( maxTileX );
        region.Max( t => t.Y ).Should().Be( maxTileY );

        foreach( var tile in region.Where( t => t.InProjection ) )
        {
            tile.ImageBytes.Should().BePositive();
        }
    }

    [ Theory ]
    [ InlineData( 0, 4, 2, 0, 0 ) ]
    [ InlineData( 0, 4, 2, -1, 3 ) ]
    [ InlineData( 0, 4, 2, -2, 2 ) ]
    [ InlineData( 0, 3, 2, -2, 2 ) ]
    [ InlineData( 0, 3, 2, -1, 3 ) ]
    [ InlineData( 0, 4, 2, 15, -1 ) ]
    [ InlineData( 0, 3, 2, 15, -1 ) ]
    [ InlineData( 1, 2, 2, -15, -1 ) ]
    public void AbsoluteTile( int regionStart, int regionWidth, int scale, int relativeX, int absoluteX )
    {
        var projection = CreateAndAuthenticateProjection( "BingMaps" );
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var width = regionWidth * projection.TileHeightWidth;

        var region = new MapRegion(projection, LoggerFactory)
                    .Size(projection.TileHeightWidth, width)
                    .Scale(scale);

        // since y tile is always 0, center is halfway down the first row
        var center = new MapPoint(region);
        center.SetCartesian( regionStart * projection.TileHeightWidth + width / 2, projection.TileHeightWidth / 2 );

        region.Center( center.Latitude, center.Longitude )
                    .Update();

        var mapTile = new MapTile( region, 0 ).SetXRelative( relativeX );

        mapTile.X.Should().Be( absoluteX );
        mapTile.Y.Should().Be( 0 );
    }
}