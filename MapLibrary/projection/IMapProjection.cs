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
        double viewPortWidth,
        double viewPortHeight,
        double rotation
    );

    BoundingBox GetBoundingBox( LatLong center, double viewportWidth, double viewportHeight, double rotation );

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
    
    MapTile GetTileFromScreenPoint( Point point );

    MapTile GetTileFromLatLong(
        LatLong latLong,
        double offsetX = 0,
        double offsetY = 0
    );

    MapTile GetTileCoordinates( int xTile, int yTile);
    Point ToScreenPoint( MapTile mapTile );
}
