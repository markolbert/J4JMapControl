namespace J4JMapLibrary;

public interface IMapProjection
{
    string Name { get; }
    MapScope GetScope();

    int MaxRequestLatency { get; set; }
    bool Initialized { get; }

    Task<bool> Authenticate( string? credentials = null );
    Task<bool> Authenticate( CancellationToken cancellationToken, string? credentials = null );
}

public interface IMapProjection<out TScope> : IMapProjection
    where TScope : MapScope
{
    TScope Scope { get; }
}