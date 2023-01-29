namespace J4JMapLibrary;

public interface IMapProjection
{
    string Name { get; }
    int MaxRequestLatency { get; set; }
    bool Initialized { get; }

    ProjectionMetrics Metrics { get; }

    int Width { get; }
    int Height { get; }

    Task<bool> Authenticate( string? credentials = null );
    Task<bool> Authenticate( CancellationToken cancellationToken, string? credentials = null );
}