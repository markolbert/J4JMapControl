namespace J4JSoftware.MapLibrary;

public interface IZoom : IPointValueLimits<int>
{
    event EventHandler<int>? Changed;

    MapRetrieverInfo? MapRetrieverInfo { get; }
    int Level { get; set; }

    int RetrievalBitmapWidthHeight { get; }
    int WidthHeight { get; }
    int NumTiles { get; }

    AbsolutePixelPoint TileToAbsolutePoint( IntPoint tile );
    IntPoint AbsolutePointToTile( AbsolutePixelPoint point );

    LatLong RelativePointToLatLong( RelativePixelPoint point );
    RelativePixelPoint LatLongToRelativePoint( LatLong latLong );
}
