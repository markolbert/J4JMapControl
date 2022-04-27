namespace J4JSoftware.MapLibrary;

public interface IZoom : IPointValueLimits<int>
{
    int Level { get; }
    int MaxLevel { get; }

    int RetrievalBitmapWidthHeight { get; }
    int WidthHeight { get; }
    int NumTiles { get; }

    IntPoint TileToScreen( IntPoint tile );
    IntPoint ScreenToTile( IntPoint screen );

    LatLong ToLatLong( IntPoint screen );
    IntPoint LatLongToScreen( LatLong latLong );
}
