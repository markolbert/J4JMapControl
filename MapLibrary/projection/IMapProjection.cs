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
        double rotation,
        AngleMeasure angleMeasure = AngleMeasure.Degrees
    );

    double LatitudeToCartesian( double angle, AngleMeasure angleMeasure = AngleMeasure.Degrees );
    double LongitudeToCartesian( double angle, AngleMeasure angleMeasure = AngleMeasure.Degrees );
    double[] LatLongToCartesian( LatLong latLong, AngleMeasure angleMeasure = AngleMeasure.Degrees );
    double CartesianToLatitude( double y, AngleMeasure angleMeasure = AngleMeasure.Degrees );
    double CartesianToLongitude( double x, AngleMeasure angleMeasure = AngleMeasure.Degrees );
    LatLong CartesianToLatLong(
        double x,
        double y,
        AngleMeasure angleMeasure = AngleMeasure.Degrees
    );

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
