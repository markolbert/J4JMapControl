namespace J4JSoftware.MapLibrary;

public interface IZoom : IPointValueLimits<int>
{
    event EventHandler<int>? Changed;

    MapRetrieverInfo? MapRetrieverInfo { get; }
    int Level { get; set; }

    int RetrievalBitmapWidthHeight { get; }
    int WidthHeight { get; }
    int NumTiles { get; }

    DoublePoint TileToPixel( IntPoint tile );
    IntPoint PixelToTile( DoublePoint screen );

    LatLong PixelToLatLong( DoublePoint screen );
    DoublePoint LatLongToPixel( LatLong latLong );
}
