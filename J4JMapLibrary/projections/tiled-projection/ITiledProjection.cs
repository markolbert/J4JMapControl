using static System.Formats.Asn1.AsnWriter;

namespace J4JMapLibrary;

public interface ITiledProjection : IProjection
{
    int Width { get; }
    int Height { get; }

    MinMax<int> TileXRange { get; }
    MinMax<int> TileYRange { get; }

    ITiledScale TiledScale { get; }
    ITileCache? TileCache { get; }

    float GroundResolution( float latitude );
    string ScaleDescription( float latitude, float dotsPerInch );

    Task<List<ITiledFragment>?> GetViewportRegionAsync(
        Viewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );

    Task<TiledMapExtract?> GetViewportTilesAsync(
        Viewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );
}
