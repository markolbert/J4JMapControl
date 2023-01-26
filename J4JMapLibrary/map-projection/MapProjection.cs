using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract partial class MapProjection : IMapProjection
{
    // thanx to Benjamin Hodgson, Ray Burns, Regent et al for this!
    // https://stackoverflow.com/questions/1664793/how-to-restrict-access-to-nested-class-member-to-enclosing-class
#pragma warning disable CS8618
    protected static Func<IMapProjection, IJ4JLogger, MapPoint> CreateMapPointInternal;
    protected static Func<IMapProjection, IJ4JLogger, LatLong> CreateLatLongInternal;
    protected static Func<IMapProjection, IJ4JLogger, Cartesian> CreateCartesianInternal;
#pragma warning restore CS8618

    private readonly ILibraryConfiguration? _libConfiguration;

    protected MapProjection(
        ISourceConfiguration srcConfig,
        IJ4JLogger logger
    )
    {
        // thanx to Benjamin Hodgson, Ray Burns, Regent et al for this!
        // https://stackoverflow.com/questions/1664793/how-to-restrict-access-to-nested-class-member-to-enclosing-class
        RuntimeHelpers.RunClassConstructor(typeof(MapPoint).TypeHandle);

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
        // thanx to Benjamin Hodgson, Ray Burns, Regent et al for this!
        // https://stackoverflow.com/questions/1664793/how-to-restrict-access-to-nested-class-member-to-enclosing-class
        RuntimeHelpers.RunClassConstructor(typeof(MapPoint).TypeHandle);

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

        var srcConfig = GetSourceConfiguration<ISourceConfiguration>( Name );

        if( srcConfig == null )
        {
            Logger.Fatal( "No configuration information for {0} was found in ILibraryConfiguration", GetType() );
            throw new ApplicationException(
                $"No configuration information for {GetType()} was found in ILibraryConfiguration" );
        }

        Copyright = srcConfig.Copyright;
        CopyrightUri = srcConfig.CopyrightUri;

        MaxLatitude = srcConfig.MaxLatitude;
        MinLatitude = srcConfig.MinLatitude;
        MaxLongitude = srcConfig.MaxLongitude;
        MinLongitude = srcConfig.MinLongitude;
    }

    protected IJ4JLogger Logger { get; }

    protected T? GetSourceConfiguration<T>( string name )
        where T : class, ISourceConfiguration
    {
        var retVal = _libConfiguration?.SourceConfigurations
                                       .FirstOrDefault( x => x.Name.Equals( Name,
                                                                            StringComparison.OrdinalIgnoreCase ) )
            as T;

        return retVal;
    }

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

    protected List<MapPoint> RegisteredPoints { get; } = new();

    public virtual MapPoint CreateMapPoint()
    {
        var retVal = CreateMapPointInternal(this, Logger);
        RegisteredPoints.Add(retVal);

        return retVal;
    }

    public LatLong CartesianToLatLong( int x, int y )
    {
        var retVal = CreateLatLongInternal( this, Logger );

        if( !Initialized )
        {
            Logger.Error( "Not initialized" );
            return retVal;
        }

        x = MapExtensions.ConformValueToRange( x, MinX, MaxX, "X coordinate", Logger );
        y = MapExtensions.ConformValueToRange( y, MinY, MaxY, "Y coordinate", Logger );

        retVal.Latitude = ( 2 * Math.Atan( Math.Exp( MapConstants.TwoPi * y / Height ) ) - MapConstants.HalfPi )
          / MapConstants.RadiansPerDegree;
        retVal.Longitude = 360 * x / Width - 180;

        return retVal;
    }

    public Cartesian LatLongToCartesian(double latitude, double longitude)
    {
        var retVal = CreateCartesianInternal(this, Logger );

        if (!Initialized)
        {
            Logger.Error("Not initialized");
            return retVal;
        }

        latitude = MapExtensions.ConformValueToRange( latitude, MinLatitude, MaxLatitude, "Latitude", Logger );
        longitude = MapExtensions.ConformValueToRange( longitude, MinLongitude, MaxLongitude, "Longitude", Logger );

        var x = Width * (longitude / 360 + 0.5);
        var y = Width * Math.Log(Math.Tan(MapConstants.QuarterPi + latitude * MapConstants.RadiansPerDegree / 2)) / MapConstants.TwoPi;

        try
        {
            retVal.X = Convert.ToInt32( Math.Round( x ) );
            retVal.Y = Convert.ToInt32( Math.Round( y ) );
        }
        catch (Exception ex)
        {
            Logger.Error<string>("Could not convert double to int32, message was '{0}'", ex.Message);
        }

        return retVal;
    }
}
