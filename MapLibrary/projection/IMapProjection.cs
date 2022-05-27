using Windows.Foundation;

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

    Point LatLongToScreen(LatLong latLong);
    LatLong ScreenToLatLong( Point screenPoint );
    LatLong Offset( LatLong origin, double xOffset, double yOffset );

    Point ToUpperLeftOrigin( Point point );
    
    TilePoint GetTileFromScreenPoint( Point point );

    TilePoint GetTileFromLatLong(
        LatLong latLong,
        double offsetX = 0,
        double offsetY = 0
    );

    MultiCoordinates GetTileCoordinates( int xTile, int yTile, CoordinateOrigin origin);
    Point ToScreenPoint( TilePoint tilePoint );
}
