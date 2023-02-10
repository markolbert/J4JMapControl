namespace J4JMapLibrary;

public interface IStaticProjection : IMapProjection
{
    float GroundResolution( float latitude );
    string MapScale( float latitude, float dotsPerInch );

    Task<List<IStaticFragment>?> GetViewportRegionAsync(
        Viewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );

    Task<StaticExtract?> GetViewportTilesAsync(
        Viewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );
}

public interface IStaticProjection<out TScope> : IStaticProjection
    where TScope : MapScope
{
    TScope Scope { get; }
}
