using Windows.Foundation;

namespace J4JSoftware.MapLibrary;

public interface IMapProjection
{
    MapRetrieverInfo MapRetrieverInfo { get; }

    int ZoomLevel { get; set; }
    int ZoomFactor { get; }

    double ViewportWidth { get; set; }
    int ProjectionWidthHeight { get; }
    int TileWidthHeight { get; }

    Rect GetProjectionRegion( BoundingBox boundingBox );
    TileRegion GetTileRegion( BoundingBox boundingBox );

    BoundingBox GetBoundingBox( LatLong center, double viewportWidth, double viewportHeight, double rotation );

    Point LatLongToCartesian( LatLong latLong );
    LatLong CartesianToLatLong( Point screenPoint );

    LatLong Offset( LatLong origin, double xOffset, double yOffset );

    Point ToUpperLeftOrigin( Point point );
    
    Point MapTileToCartesian( MapTile mapTile );
    Point MapTileCenterToCartesian( MapTile mapTile );
}
