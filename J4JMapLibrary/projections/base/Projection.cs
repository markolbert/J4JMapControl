using System.Reflection;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class Projection<TAuth> : IProjection<TAuth>
    where TAuth : class
{
    protected Projection(
        IMapServer mapServer,
        ProjectionScale projectionScale,
        IJ4JLogger logger
    )
    {
        Logger = logger;
        Logger.SetLoggedType( GetType() );

        MapScale = projectionScale;

        var attribute = GetType().GetCustomAttribute<ProjectionAttribute>();
        if( attribute == null )
            Logger.Error( "Map projection class is not decorated with ProjectionAttribute(s), cannot be used" );
        else Name = attribute.Name;

        MapServer = mapServer;
    }

    protected Projection(
        IProjectionCredentials credentials,
        IMapServer mapServer,
        ProjectionScale projectionScale,
        IJ4JLogger logger
    )
    {
        Logger = logger;
        Logger.SetLoggedType( GetType() );

        var attributes = GetType().GetCustomAttributes<ProjectionAttribute>().ToList();
        if( !attributes.Any() )
        {
            Logger.Fatal( "Map projection class is not decorated with ProjectionAttribute(s)" );
            throw new ApplicationException( "Map projection class is not decorated with ProjectionAttribute(s)" );
        }

        Name = attributes.First().Name;

        LibraryConfiguration = credentials;

        MapScale = projectionScale;

        var attribute = GetType().GetCustomAttribute<ProjectionAttribute>();
        if( attribute == null )
            Logger.Error( "Map projection class is not decorated with ProjectionAttribute(s), cannot be used" );
        else Name = attribute.Name;

        MapServer = mapServer;
    }

    protected IJ4JLogger Logger { get; }
    protected IProjectionCredentials? LibraryConfiguration { get; }

    public ProjectionScale MapScale { get; }
    public IMapServer MapServer { get; }

    public virtual bool Initialized => !string.IsNullOrEmpty( Name ) && MapServer.Initialized;

    public string Name { get; } = string.Empty;

    public abstract Task<bool> AuthenticateAsync( TAuth? credentials, CancellationToken ctx = default );

    async Task<bool> IProjection.AuthenticateAsync( object? credentials, CancellationToken ctx )
    {
        switch( credentials )
        {
            case TAuth castCredentials:
                return await AuthenticateAsync( castCredentials, ctx );

            case null:
                return await AuthenticateAsync( null, ctx );

            default:
                Logger.Error( "Expected a {0} but received a {1}", typeof( TAuth ), credentials.GetType() );
                return false;
        }
    }
}
