namespace J4JMapLibrary;

public interface IMapProjection
{
    string Name { get; }

    bool Initialized { get; }

    IMapServer MapServer { get; }
    event EventHandler<int>? ScaleChanged;
    MapScope GetScope();

    void SetScale( int scale );

    Task<bool> AuthenticateAsync( object? credentials, CancellationToken ctx = default );
}

public interface IMapProjection<out TScope, in TAuth> : IMapProjection
    where TScope : MapScope
{
    TScope Scope { get; }
    Task<bool> AuthenticateAsync( TAuth? credentials, CancellationToken ctx = default );
}
