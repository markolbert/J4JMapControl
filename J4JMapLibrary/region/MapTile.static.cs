using Serilog;

namespace J4JSoftware.J4JMapLibrary.MapRegion;

public partial class MapTile
{
    public static MapTile CreateStaticMapTile(
        IProjection projection,
        int xTile,
        int yTile,
        int scale,
        ILogger logger
    )
    {
        var region = new MapRegion( projection, logger )
                    .Scale( scale )
                    .Size( projection.TileHeightWidth, projection.TileHeightWidth )
                    .Build();

        // determine the center point of the tile
        var upperLeftX = xTile * projection.TileHeightWidth;
        var upperLeftY = yTile * projection.TileHeightWidth;
        var centerPoint = new StaticPoint( projection ) { Scale = scale };
        centerPoint.SetCartesian( upperLeftX + projection.TileHeightWidth / 2,
                                  upperLeftY + projection.TileHeightWidth / 2 );

        region.CenterLatitude = centerPoint.Latitude;
        region.CenterLongitude = centerPoint.Longitude;
        region.RequestedHeight = (float) projection.TileHeightWidth / 2;
        region.RequestedWidth = (float) projection.TileHeightWidth / 2;

        return new MapTile( region, xTile, yTile );
    }

    public static MapTile CreateMapTileFromRelativeX( MapRegion region, int x, int y )
    {
        x = region.ConvertRelativeXToAbsolute( x );
        return new MapTile( region, x, y );
    }

    public static MapTile CreateMapTileFromAbsoluteX( MapRegion region, int x, int y ) => new( region, x, y );
}
