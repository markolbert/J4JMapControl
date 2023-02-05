using System.Reflection;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class MapProjection : IMapProjection
{
    private readonly ILibraryConfiguration? _libConfiguration;

    protected MapProjection(
        ISourceConfiguration srcConfig,
        IJ4JLogger logger
    )
    {
        MaxRequestLatency = srcConfig.MaxRequestLatency;

        CancellationTokenSource = new CancellationTokenSource();
        CancellationTokenSource.CancelAfter( MaxRequestLatency );

        Copyright = srcConfig.Copyright;
        CopyrightUri = srcConfig.CopyrightUri;

        ScaleRange = new MinMax<int>( 1, 1 );
        LatitudeRange = new MinMax<float>( srcConfig.MinLatitude, srcConfig.MaxLatitude );
        LongitudeRange = new MinMax<float>( srcConfig.MinLongitude, srcConfig.MaxLongitude );
        XRange = new MinMax<int>( 0, 0 );
        YRange = new MinMax<int>( 0, 0 );

        Logger = logger;
        Logger.SetLoggedType( GetType() );

        var attributes = GetType().GetCustomAttributes<MapProjectionAttribute>().ToList();
        if (!attributes.Any())
        {
            Logger.Fatal("Map projection class is not decorated with MapProjectionAttribute(s)");
            throw new ApplicationException( "Map projection class is not decorated with MapProjectionAttribute(s)" );
        }

        Name = attributes.First().Name;
    }

    protected MapProjection(
        ILibraryConfiguration libConfiguration,
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

        _libConfiguration = libConfiguration;

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

        ScaleRange = new MinMax<int>(1, 1);
        LatitudeRange = new MinMax<float>( srcConfig.MinLatitude, srcConfig.MaxLatitude );
        LongitudeRange = new MinMax<float>( srcConfig.MinLongitude, srcConfig.MaxLongitude );
        XRange = new MinMax<int>( 0, 0 );
        YRange = new MinMax<int>( 0, 0 );
    }

    protected CancellationTokenSource CancellationTokenSource { get; }
    protected IJ4JLogger Logger { get; }

    protected bool TryGetSourceConfiguration<T>( string name, out T? result )
        where T : class, ISourceConfiguration
    {
        result = _libConfiguration?.SourceConfigurations
                                       .FirstOrDefault( x => x.Name.Equals( Name,
                                                                            StringComparison.OrdinalIgnoreCase ) )
            as T;

        return result != null;
    }

    protected bool TryGetCredentials( string name, out string? result )
    {
        result = _libConfiguration?.Credentials
                                   .FirstOrDefault( x => x.Name.Equals( Name,
                                                                        StringComparison.OrdinalIgnoreCase ) )
                                  ?.Key;

        return result != null;
    }

    public abstract int Scale { get; set; }
    public MinMax<int> ScaleRange { get; protected set; }
    public MinMax<int> XRange { get; protected set; }
    public MinMax<int> YRange { get; protected set; }
    public MinMax<float> LatitudeRange { get; protected set; }
    public MinMax<float> LongitudeRange { get; protected set; }

    public int MaxRequestLatency { get; set; }
    public bool Initialized { get; protected set; }

    public string Name { get; }
    public string Copyright { get; }
    public Uri? CopyrightUri { get; }

    public int Width => XRange.Maximum - XRange.Maximum;
    public int Height => YRange.Maximum - YRange.Minimum;

    public Task<bool> Authenticate( string? credentials = null ) =>
        Authenticate( CancellationTokenSource.Token, credentials );

    public abstract Task<bool> Authenticate(CancellationToken cancellationToken, string? credentials = null);
}
