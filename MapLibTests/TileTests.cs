using FluentAssertions;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;

namespace MapLibTests;

public class TileTests : TestBase
{
    [ Theory ]
    [ InlineData( "BingMaps", 1, 0, 0, 128, 256, 0, 0, 0, 1, 1 ) ]
    [ InlineData( "OpenStreetMaps", 0, 0, 0, 128, 256, 0, -1, 0, 1, 0 ) ]
    [ InlineData( "BingMaps", 2, 37, -122, 128, 256, 0, 0, 1, 1, 1 ) ]
    [ InlineData( "BingMaps", 2, 37, -122, 128, 256, 45, 0, 1, 1, 2 ) ]
    [ InlineData( "BingMaps", 2, 37, -122, 128, 512, 45, -1, 0, 1, 2 ) ]
    [ InlineData( "BingMaps", 3, 37, -122, 512, 512, 45, -1, 1, 2, 4 ) ]
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

        region.MapTiles.Min( t => t.X ).Should().Be( minTileX );
        region.MapTiles.Min( t => t.Y ).Should().Be( minTileY );
        region.MapTiles.Max( t => t.X ).Should().Be( maxTileX );
        region.MapTiles.Max( t => t.Y ).Should().Be( maxTileY );

        foreach( var tile in region.MapTiles.Where( t => t.InProjection ) )
        {
            tile.ImageBytes.Should().BePositive();
        }
    }

    [ Theory ]
    [ InlineData( 0, 4, 2, 0, 0 ) ]
    [ InlineData( 0, 4, 2, -1, 3 ) ]
    [ InlineData( 0, 4, 2, -2, -1 ) ]
    [ InlineData( 0, 3, 2, -2, -1 ) ]
    [ InlineData( 0, 3, 2, -1, 3 ) ]
    [ InlineData( 0, 4, 2, 15, -1 ) ]
    [ InlineData( 0, 3, 2, 15, -1 ) ]
    [ InlineData( 1, 2, 2, -15, -1 ) ]
    public async Task AbsoluteTile( int regionStart, int regionWidth, int scale, int x, int absX )
    {
        var projection = await CreateProjection( "BingMaps" );
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var width = regionWidth * projection.TileHeightWidth;
        var center = new StaticPoint( projection ) { Scale = scale };
        center.SetCartesian( regionStart * projection.TileHeightWidth + projection.TileHeightWidth / 2, null );

        var region = new MapRegion( projection, Logger )
                    .Center( center.Latitude, center.Longitude )
                    .Size( projection.TileHeightWidth, width )
                    .Scale( scale )
                    .Build();

        var mapTile = new MapTile( region, x, 0 );

        var absolute = mapTile.AbsoluteTileCoordinates;
        absolute.X.Should().Be( absX );
        absolute.Y.Should().Be( 0 );
    }
}