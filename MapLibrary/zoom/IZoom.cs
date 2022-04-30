namespace J4JSoftware.MapLibrary;

public interface IZoom : IPointValueLimits<int>
{
    event EventHandler<int>? Changed;

    MapRetrieverInfo? MapRetrieverInfo { get; }
    int Level { get; set; }

    int RetrievalBitmapWidthHeight { get; }
    int WidthHeight { get; }
    int NumTiles { get; }

    DoublePoint TileToScreen( IntPoint tile );
    IntPoint ScreenToTile( DoublePoint screen );

    LatLong ScreenToLatLong( DoublePoint screen );
    IntPoint LatLongToScreen( LatLong latLong );
}
