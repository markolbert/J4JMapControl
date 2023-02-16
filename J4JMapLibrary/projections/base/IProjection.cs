namespace J4JMapLibrary;

public interface IProjection
{
    string Name { get; }

    bool Initialized { get; }

    IMapServer MapServer { get; }
    IProjectionScale MapScale { get; }

    Task<bool> AuthenticateAsync( object? credentials, CancellationToken ctx = default );

    IAsyncEnumerable<IMapFragment> GetExtractAsync(
        INormalizedViewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );
}

public interface IProjection<in TAuth, in TViewport, out TFrag> : IProjection
    where TAuth : class
    where TViewport : INormalizedViewport
    where TFrag : IMapFragment
{
    Task<bool> AuthenticateAsync( TAuth? credentials, CancellationToken ctx = default );

    IAsyncEnumerable<TFrag> GetExtractAsync(
        TViewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );
}
