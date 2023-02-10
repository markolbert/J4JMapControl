using static System.Formats.Asn1.AsnWriter;

namespace J4JMapLibrary;

public interface ITiledProjection : IMapProjection
{
    int Width { get; }
    int Height { get; }

    MinMax<int> TileXRange { get; }
    MinMax<int> TileYRange { get; }

    ITileCache? TileCache { get; }

    float GroundResolution( float latitude );
    string MapScale( float latitude, float dotsPerInch );

    Task<List<ITiledFragment>?> GetViewportRegionAsync(
        Viewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );

    Task<TiledExtract?> GetViewportTilesAsync(
        Viewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );
}

public interface ITiledProjection<out TScope> : ITiledProjection
    where TScope : TiledScope
{
    TScope Scope { get; }
}
