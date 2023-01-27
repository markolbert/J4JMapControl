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

        MaxLatitude = srcConfig.MaxLatitude;
        MinLatitude = srcConfig.MinLatitude;
        MaxLongitude = srcConfig.MaxLongitude;
        MinLongitude = srcConfig.MinLongitude;

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

        MaxLatitude = srcConfig.MaxLatitude;
        MinLatitude = srcConfig.MinLatitude;
        MaxLongitude = srcConfig.MaxLongitude;
        MinLongitude = srcConfig.MinLongitude;
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

    public abstract Task<bool> Authenticate( string? credentials = null );

    public bool Initialized { get; protected set; }

    public string Name { get; }
    public string Copyright { get; }
    public Uri? CopyrightUri { get; }

    public double MaxLatitude { get; }
    public double MinLatitude { get; }
    public double MaxLongitude { get; }
    public double MinLongitude { get; }

    public int MinX { get; protected set; }
    public int MaxX { get; protected set; }
    public int MinY { get; protected set; }
    public int MaxY { get; protected set; }

    public int Width => MaxX - MinX;
    public int Height => MaxY - MinY;
}
