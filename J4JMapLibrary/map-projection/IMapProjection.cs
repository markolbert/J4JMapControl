namespace J4JMapLibrary;

public interface IMapProjection
{
    event EventHandler<int>? ScaleChanged;

    string Name { get; }
    MapScope GetScope();

    int MaxRequestLatency { get; set; }
    bool Initialized { get; }

    void SetScale(int scale);

    IMapServer MapServer { get; }

    Task<bool> AuthenticateAsync(object? credentials);
    Task<bool> AuthenticateAsync(object? credentials, CancellationToken cancellationToken);
}

public interface IMapProjection<out TScope, in TAuth> : IMapProjection
    where TScope : MapScope
{
    TScope Scope { get; }
    Task<bool> AuthenticateAsync(TAuth? credentials);
    Task<bool> AuthenticateAsync(TAuth? credentials, CancellationToken cancellationToken);
}