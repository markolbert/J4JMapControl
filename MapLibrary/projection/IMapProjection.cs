namespace J4JSoftware.MapLibrary;

public interface IMapProjection
{
    event EventHandler<int>? ZoomChanged;

    MapRetrieverInfo? MapRetrieverInfo { get; }
    
    public int MaximumZoom { get; }
    public int MinimumZoom { get; }
    int ZoomLevel { get; set; }

    double MapWidth { get; set; }
    double MaximumMapHeight { get; }
    double MapHeight { get; set; }

    int ProjectionWidthHeight { get; }
    int TileWidthHeight { get; }
    int NumTiles { get; }

    double LongitudeFromScreen( double screenX );
    double ScreenFromLongitude( double longDegrees );
    double LatitudeFromScreen( double screenY );
    double ScreenFromLatitude( double latDegrees );

    LatLong ScreenPointToLatLong( DoublePoint point );
    DoublePoint LatLongToScreenPoint( LatLong latLong );

    TilePoint GetTileFromScreenPoint(DoublePoint point);
    TilePoint GetTileFromLatLong(LatLong latLong);
}
