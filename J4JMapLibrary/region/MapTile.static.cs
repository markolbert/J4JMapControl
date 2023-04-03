using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary.MapRegion;

public partial class MapTile
{
    public static MapTile CreateStaticMapTile(
        IProjection projection,
        int xTile,
        int yTile,
        int scale,
        ILoggerFactory? loggerFactory
    )
    {
        var region = new MapRegion( projection, loggerFactory )
                    .Scale( scale )
                    .Size( projection.TileHeightWidth, projection.TileHeightWidth )
                    .Update();

        // determine the center point of the tile
        var upperLeftX = xTile * projection.TileHeightWidth;
        var upperLeftY = yTile * projection.TileHeightWidth;
        var centerPoint = new MapPoint( region );
        centerPoint.SetCartesian( upperLeftX + projection.TileHeightWidth / 2,
                                  upperLeftY + projection.TileHeightWidth / 2 );

        region.CenterLatitude = centerPoint.Latitude;
        region.CenterLongitude = centerPoint.Longitude;
        region.RequestedHeight = (float) projection.TileHeightWidth / 2;
        region.RequestedWidth = (float) projection.TileHeightWidth / 2;

        return new MapTile( region, yTile ).SetXAbsolute( 0 );
    }
}
