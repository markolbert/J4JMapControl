using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        Copyright = srcConfig.Copyright;
        CopyrightUri = srcConfig.CopyrightUri;

        Metrics = new ProjectionMetrics()
        {
            LatitudeRange = new MinMax<double>( srcConfig.MinLatitude, srcConfig.MaxLatitude ),
            LongitudeRange = new MinMax<double>( srcConfig.MinLongitude, srcConfig.MaxLongitude ),
            XRange = new MinMax<int>( 0, 0 ),
            YRange = new MinMax<int>( 0, 0 )
        };

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

        Copyright = srcConfig!.Copyright;
        CopyrightUri = srcConfig.CopyrightUri;

        Metrics = new ProjectionMetrics()
        {
            LatitudeRange = new MinMax<double>(srcConfig.MinLatitude, srcConfig.MaxLatitude),
            LongitudeRange = new MinMax<double>(srcConfig.MinLongitude, srcConfig.MaxLongitude),
            XRange = new MinMax<int>(0, 0),
            YRange = new MinMax<int>(0, 0)
        };
    }

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

    public ProjectionMetrics Metrics { get; protected set; }

    public abstract Task<bool> Authenticate( string? credentials = null );

    public bool Initialized { get; protected set; }

    public string Name { get; }
    public string Copyright { get; }
    public Uri? CopyrightUri { get; }

    public int Width => Metrics.XRange.Maximum - Metrics.XRange.Maximum;
    public int Height => Metrics.YRange.Maximum - Metrics.YRange.Minimum;
}
