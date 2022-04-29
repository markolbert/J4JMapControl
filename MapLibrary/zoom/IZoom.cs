namespace J4JSoftware.MapLibrary;

public interface IZoom : IPointValueLimits<int>
{
    event EventHandler<int>? Changed;

    MapRetrieverInfo? MapRetrieverInfo { get; }
    int Level { get; set; }

    int RetrievalBitmapWidthHeight { get; }
    int WidthHeight { get; }
    int NumTiles { get; }

    IntPoint TileToScreen( IntPoint tile );
    IntPoint ScreenToTile( IntPoint screen );

    LatLong ToLatLong( IntPoint screen );
    IntPoint LatLongToScreen( LatLong latLong );
}
