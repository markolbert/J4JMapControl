using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public abstract class TileBasedImageRetriever<TMultiTile> : MapImageRetriever<TMultiTile>
    where TMultiTile : MultiTileCoordinates
{
    protected TileBasedImageRetriever(
        MapRetrieverInfo mapRetrieverInfo,
        string retrievalUriTemplate,
        IApplicationInfo appInfo,
        IJ4JLogger? logger
    )
        : base( mapRetrieverInfo, appInfo, logger )
    {
        RetrievalUriTemplate = retrievalUriTemplate;
    }

    public string RetrievalUriTemplate { get; }
}
