using System.Reflection;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class MapProjection<TScope, TAuth> : IMapProjection<TScope, TAuth>
    where TScope : MapScope, new()
    where TAuth : class
{
    public event EventHandler<int>? ScaleChanged;

    protected MapProjection(
        ISourceConfiguration srcConfig,
        IMapServer mapServer,
        IJ4JLogger logger
    )
    {
        MaxRequestLatency = srcConfig.MaxRequestLatency;

        CancellationTokenSource = new CancellationTokenSource();
        CancellationTokenSource.CancelAfter( MaxRequestLatency );

        Copyright = srcConfig.Copyright;
        CopyrightUri = srcConfig.CopyrightUri;

        Scope = new TScope
        {
            LatitudeRange = new MinMax<float>( srcConfig.MinLatitude, srcConfig.MaxLatitude ),
            LongitudeRange = new MinMax<float>( srcConfig.MinLongitude, srcConfig.MaxLongitude )
        };

        Logger = logger;
        Logger.SetLoggedType( GetType() );

        var attribute = GetType().GetCustomAttribute<MapProjectionAttribute>();
        if (attribute == null)
            Logger.Error("Map projection class is not decorated with MapProjectionAttribute(s), cannot be used");
        else Name = attribute.Name;

        MapServer = mapServer;
    }

    protected MapProjection(
        ILibraryConfiguration libConfiguration,
        IMapServer mapServer,
        IJ4JLogger logger
    )
    {
        Logger = logger;
        Logger.SetLoggedType(GetType());

        var attributes = GetType().GetCustomAttributes<MapProjectionAttribute>().ToList();
        if (!attributes.Any())
        {
            Logger.Fatal("Map projection class is not decorated with MapProjectionAttribute(s)");
            throw new ApplicationException("Map projection class is not decorated with MapProjectionAttribute(s)");
        }

        Name = attributes.First().Name;

        LibraryConfiguration = libConfiguration;

        if( !TryGetSourceConfiguration<ISourceConfiguration>(Name, out var srcConfig ))
        {
            Logger.Fatal( "No configuration information for {0} was found in ILibraryConfiguration", GetType() );
            throw new ApplicationException(
                $"No configuration information for {GetType()} was found in ILibraryConfiguration" );
        }

        MaxRequestLatency = srcConfig!.MaxRequestLatency;

        CancellationTokenSource = new CancellationTokenSource();
        CancellationTokenSource.CancelAfter(MaxRequestLatency);

        Copyright = srcConfig.Copyright;
        CopyrightUri = srcConfig.CopyrightUri;

        Scope = new TScope
        {
            LatitudeRange = new MinMax<float>(srcConfig.MinLatitude, srcConfig.MaxLatitude),
            LongitudeRange = new MinMax<float>(srcConfig.MinLongitude, srcConfig.MaxLongitude)
        };

        var attribute = GetType().GetCustomAttribute<MapProjectionAttribute>();
        if (attribute == null)
            Logger.Error("Map projection class is not decorated with MapProjectionAttribute(s), cannot be used");
        else Name = attribute.Name;

        MapServer = mapServer;
    }

    protected CancellationTokenSource CancellationTokenSource { get; }
    protected IJ4JLogger Logger { get; }
    protected ILibraryConfiguration? LibraryConfiguration { get; }

    protected bool TryGetSourceConfiguration<T>( string name, out T? result )
        where T : class, ISourceConfiguration
    {
        result = LibraryConfiguration?.SourceConfigurations
                                       .FirstOrDefault( x => x.Name.Equals( Name,
                                                                            StringComparison.OrdinalIgnoreCase ) )
            as T;

        return result != null;
    }

    public TScope Scope { get; }
    public IMapServer MapServer { get; }

    public int MaxRequestLatency { get; set; }
    public virtual bool Initialized => !string.IsNullOrEmpty(Name) && MapServer.Initialized;

    public string Name { get; } = string.Empty;
    public string Copyright { get; }
    public Uri? CopyrightUri { get; }

    public void SetScale(int scale)
    {
        if (!Initialized)
        {
            Logger.Error("Trying to set scale before projection is initialized, ignoring");
            return;
        }

        Scope.Scale = Scope.ScaleRange.ConformValueToRange(scale, "Scale");

        SetSizes(scale);

        ScaleChanged?.Invoke(this, scale);
    }

    // this assumes MapServer has been set and scale is valid
    protected virtual void SetSizes(int scale)
    {
    }

    public Task<bool> AuthenticateAsync( TAuth? credentials ) =>
        AuthenticateAsync( credentials, CancellationToken.None );

    public abstract Task<bool> AuthenticateAsync(TAuth? credentials, CancellationToken cancellationToken);

    MapScope IMapProjection.GetScope() => Scope;

    async Task<bool> IMapProjection.AuthenticateAsync(object? credentials)
    {
        switch (credentials)
        {
            case TAuth castCredentials:
                return await AuthenticateAsync(castCredentials);

            case null:
                return await AuthenticateAsync(null);

            default:
                Logger.Error("Expected a {0} but received a {1}", typeof(TAuth), credentials.GetType());
                return false;
        }
    }

    async Task<bool> IMapProjection.AuthenticateAsync(object? credentials, CancellationToken cancellationToken)
    {
        switch (credentials)
        {
            case TAuth castCredentials:
                return await AuthenticateAsync(castCredentials, cancellationToken);

            case null:
                return await AuthenticateAsync(null, cancellationToken);

            default:
                Logger.Error("Expected a {0} but received a {1}", typeof(TAuth), credentials.GetType());
                return false;
        }
    }
}
