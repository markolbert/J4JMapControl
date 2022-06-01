using Windows.Foundation;

namespace J4JSoftware.MapLibrary;

public interface IMapProjection
{
    MapRetrieverInfo MapRetrieverInfo { get; }
    //public int MaximumZoom { get; }
    //public int MinimumZoom { get; }
    int ZoomLevel { get; set; }
    int ZoomFactor { get; }
    double ViewportWidth { get; set; }
    //double MapRadius { get; }

    int ProjectionWidthHeight { get; }
    int TileWidthHeight { get; }

    Rect GetProjectionRegion( BoundingBox boundingBox );
    TileRegion GetTileRegion( BoundingBox boundingBox );

    BoundingBox GetBoundingBox( LatLong center, double viewportWidth, double viewportHeight, double rotation );

    Point LatLongToCartesian( LatLong latLong );
    LatLong CartesianToLatLong( Point screenPoint );

    //Point LatLongToScreen(LatLong latLong);
    //LatLong ScreenToLatLong( Point screenPoint );
    LatLong Offset( LatLong origin, double xOffset, double yOffset );

    Point ToUpperLeftOrigin( Point point );
    
    //MapTile GetTileFromScreenPoint( Point point );

    //MapTile GetTileFromLatLong(
    //    LatLong latLong,
    //    double offsetX = 0,
    //    double offsetY = 0
    //);

    //MapTile GetTileCoordinates( int xTile, int yTile);
    Point MapTileToCartesian( MapTile mapTile );
}
