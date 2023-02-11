namespace J4JMapLibrary;

public interface IProjection
{
    string Name { get; }

    bool Initialized { get; }

    IMapServer MapServer { get; }
    ProjectionScale MapScale { get; }

    Task<bool> AuthenticateAsync( object? credentials, CancellationToken ctx = default );
}

public interface IProjection<in TAuth> : IProjection
    where TAuth : class
{
    Task<bool> AuthenticateAsync( TAuth? credentials, CancellationToken ctx = default );
}
