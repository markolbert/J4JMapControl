namespace J4JSoftware.MapLibrary;

public interface IMapProjection
{
    MapRetrieverInfo MapRetrieverInfo { get; }

    public int MaximumZoom { get; }
    public int MinimumZoom { get; }
    int ZoomLevel { get; set; }

    double ViewportWidth { get; set; }
    double MapRadius { get; }

    int ProjectionWidthHeight { get; }
    int TileWidthHeight { get; }
    int ZoomFactor { get; }
    TileRegion GetTileRegion(
        LatLong center,
        double boundingBoxWidth,
        double boundingBoxHeight,
        double rotation
    );

    double LatitudeToCartesian( double latitude );
    double LongitudeToCartesian( double longitude );
    double[] LatLongToCartesian( LatLong latLong );
    double CartesianToLatitude( double y );
    double CartesianToLongitude( double x );
    LatLong CartesianToLatLong( double x, double y );

    DoublePoint LatLongToScreen(LatLong latLong, CoordinateOrigin origin);
    LatLong ScreenToLatLong( DoublePoint screenPoint );
    LatLong Offset( LatLong origin, double xOffset, double yOffset );

    double ChangeOrigin( double value, CoordinateAxis axis );
    
    TilePoint GetTileFromScreenPoint( DoublePoint point );

    TilePoint GetTileFromLatLong(
        LatLong latLong,
        double offsetX = 0,
        double offsetY = 0
    );

    MultiCoordinates GetTileCoordinates( int xTile, int yTile, CoordinateOrigin origin);
}
