namespace J4JMapLibrary;

public interface IStaticProjection : IProjection
{
    Task<List<IStaticFragment>?> GetViewportRegionAsync(
        NormalizedViewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );

    Task<StaticExtract?> GetExtractAsync(
        INormalizedViewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );
}

public interface IStaticProjection<out TScope> : IStaticProjection
    where TScope : ProjectionScale
{
    TScope Scope { get; }
}
