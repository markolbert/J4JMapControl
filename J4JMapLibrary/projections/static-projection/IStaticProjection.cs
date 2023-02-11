namespace J4JMapLibrary;

public interface IStaticProjection : IProjection
{
    Task<List<IStaticFragment>?> GetViewportRegionAsync(
        Viewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );

    Task<StaticMapExtract?> GetExtractAsync(
        Viewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );
}

public interface IStaticProjection<out TScope> : IStaticProjection
    where TScope : ProjectionScale
{
    TScope Scope { get; }
}
