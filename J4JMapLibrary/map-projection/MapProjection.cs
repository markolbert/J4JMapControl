using System.Reflection;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class MapProjection<TScope, TAuth> : IMapProjection<TScope, TAuth>
    where TScope : MapScope, new()
    where TAuth : class
{
    protected MapProjection(
        IMapServer mapServer,
        IJ4JLogger logger
    )
    {
        Scope = new TScope
        {
            LatitudeRange = new MinMax<float>( mapServer.MinLatitude, mapServer.MaxLatitude ),
            LongitudeRange = new MinMax<float>( mapServer.MinLongitude, mapServer.MaxLongitude )
        };

        Logger = logger;
        Logger.SetLoggedType( GetType() );

        var attribute = GetType().GetCustomAttribute<MapProjectionAttribute>();
        if( attribute == null )
            Logger.Error( "Map projection class is not decorated with MapProjectionAttribute(s), cannot be used" );
        else Name = attribute.Name;

        MapServer = mapServer;
    }

    protected MapProjection(
        IProjectionCredentials credentials,
        IMapServer mapServer,
        IJ4JLogger logger
    )
    {
        Logger = logger;
        Logger.SetLoggedType( GetType() );

        var attributes = GetType().GetCustomAttributes<MapProjectionAttribute>().ToList();
        if( !attributes.Any() )
        {
            Logger.Fatal( "Map projection class is not decorated with MapProjectionAttribute(s)" );
            throw new ApplicationException( "Map projection class is not decorated with MapProjectionAttribute(s)" );
        }

        Name = attributes.First().Name;

        LibraryConfiguration = credentials;

        Scope = new TScope
        {
            LatitudeRange = new MinMax<float>( mapServer.MinLatitude, mapServer.MaxLatitude ),
            LongitudeRange = new MinMax<float>( mapServer.MinLongitude, mapServer.MaxLongitude )
        };

        var attribute = GetType().GetCustomAttribute<MapProjectionAttribute>();
        if( attribute == null )
            Logger.Error( "Map projection class is not decorated with MapProjectionAttribute(s), cannot be used" );
        else Name = attribute.Name;

        MapServer = mapServer;
    }

    protected IJ4JLogger Logger { get; }
    protected IProjectionCredentials? LibraryConfiguration { get; }
    public event EventHandler<int>? ScaleChanged;

    public TScope Scope { get; }
    public IMapServer MapServer { get; }

    public virtual bool Initialized => !string.IsNullOrEmpty( Name ) && MapServer.Initialized;

    public string Name { get; } = string.Empty;

    public void SetScale( int scale )
    {
        if( !Initialized )
        {
            Logger.Error( "Trying to set scale before projection is initialized, ignoring" );
            return;
        }

        Scope.Scale = Scope.ScaleRange.ConformValueToRange( scale, "Scale" );

        SetSizes( scale );

        ScaleChanged?.Invoke( this, scale );
    }

    public abstract Task<bool> AuthenticateAsync( TAuth? credentials, CancellationToken ctx = default );

    MapScope IMapProjection.GetScope() => Scope;

    async Task<bool> IMapProjection.AuthenticateAsync( object? credentials, CancellationToken ctx )
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

    // this assumes MapServer has been set and scale is valid
    protected virtual void SetSizes( int scale )
    {
    }
}
