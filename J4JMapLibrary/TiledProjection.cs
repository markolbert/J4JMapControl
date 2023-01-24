using J4JSoftware.Logging;
using System.Net;

namespace J4JMapLibrary;

public abstract class TiledProjection : ITiledProjection
{
    public const double TwoPi = Math.PI * 2;
    public const double HalfPi = Math.PI /2;
    public const double QuarterPi = Math.PI / 4;
    public const double RadiansPerDegree = Math.PI / 180;
    public const double DegreesPerRadian = 180 / Math.PI;
    public const double EarthCircumferenceMeters = 6378137;
    public const double MetersPerInch = 0.0254;

    // thanx to 3dGrabber for this
    // https://stackoverflow.com/questions/383587/how-do-you-do-integer-exponentiation-in-c
    public static int Pow(int numBase, int exp) =>
        Enumerable
           .Repeat(numBase, Math.Abs(exp))
           .Aggregate(1, (a, b) => exp < 0 ? a / b : a * b);

    protected TiledProjection(
        double maxLatitude,
        double minLatitude,
        double minLongitude,
        double maxLongitude,
        IJ4JLogger logger
    )
    {
        Logger = logger;
        Logger.SetLoggedType( GetType() );

        MaxLatitude = maxLatitude;
        MinLatitude = minLatitude;
        MaxLongitude = maxLongitude;
        MinLongitude = minLongitude;
    }

    protected IJ4JLogger Logger { get; }

    protected List<MapPoint> RegisteredPoints { get; } = new();

    public double MaxLatitude { get; }
    public double MinLatitude { get; }
    public double MaxLongitude { get; }
    public double MinLongitude { get; }

    public int MinX { get; protected set; }
    public int MaxX { get; protected set; }
    public int MinY { get; protected set; }
    public int MaxY { get; protected set; }

    public TileCoordinates MinTile { get; protected set; } = new(0,0);
    public TileCoordinates MaxTile { get; protected set; } = new(0,0);

    public int MaxScale { get; protected set; }
    public int MinScale { get; protected set; }
    public virtual int Scale { get; set; }

    public RectangleSize Size { get; protected set; } = new();
    public RectangleSize TileSize { get; protected set; } = new();

    public double GroundResolution( double latitude )
    {
        if( !Initialized )
        {
            Logger.Error( "Projection is not initialized" );
            return 0;
        }

        latitude = Cap( latitude, MinLatitude, MaxLatitude, "Latitude" );
        return Math.Cos( latitude * RadiansPerDegree ) * EarthCircumferenceMeters / ( MaxX - MinX );
    }

    public string MapScale( double latitude, double dotsPerInch ) =>
        $"1 : {GroundResolution( latitude ) * dotsPerInch / MetersPerInch}";

    public virtual MapPoint CreateMapPoint()
    {
        var retVal = new MapPoint( this, Logger );
        RegisteredPoints.Add( retVal );

        return retVal;
    }

    public bool Initialized { get; protected set; }

    public (double latitude, double longitude) XYToLatLong( int x, int y )
    {
        if( !Initialized )
        {
            Logger.Error("Not initialized");
            return ( 0.0, 0.0 );
        }

        x = Cap(x, MinX, MaxX,"X coordinate");
        y = Cap(y, MinY, MaxY, "Y coordinate");

        var latitude = ( 2 * Math.Atan( Math.Exp( TwoPi * y / Size.Height ) ) - HalfPi ) / RadiansPerDegree;
        var longitude = 360 * x / Size.Width - 180;

        return (latitude,longitude);
    }

    public (int x, int y) LatLongToXY( double latitude, double longitude )
    {
        if (!Initialized)
        {
            Logger.Error("Not initialized");
            return (0, 0);
        }

        latitude = Cap( latitude, MinLatitude, MaxLatitude, "Latitude" );
        longitude = Cap( longitude, MinLongitude, MaxLongitude, "Longitude" );

        var x = Size.Width * ( longitude / 360 + 0.5 );
        var y = Size.Width * Math.Log( Math.Tan( QuarterPi + latitude * RadiansPerDegree / 2 ) ) / TwoPi;

        try
        {
            return ( Convert.ToInt32( Math.Round( x ) ), Convert.ToInt32( Math.Round( y ) ) );
        }
        catch( Exception ex )
        {
            Logger.Error<string>("Could not convert double to int32, message was '{0}'", ex.Message);
            return ( 0, 0 );
        }
    }

    public TileCoordinates XYToTileCoordinates( int x, int y )
    {
        if (!Initialized)
        {
            Logger.Error("Not initialized");
            return new TileCoordinates( 0, 0 );
        }

        x = Cap(x, MinX, MaxX, "X coordinate");
        y = Cap(y, MinY, MaxY, "Y coordinate");

        return new TileCoordinates( Convert.ToInt32( Math.Floor( x / 256.0 ) ),
                                    Convert.ToInt32( Math.Floor( y / 256.0 ) ) );
    }

    protected T Cap<T>( T toCheck, T min, T max, string name )
    where T: IComparable<T>
    {
        if( toCheck.CompareTo(min  )<0 )
        {
            Logger.Warning( "{0} ({1}) < minimum ({2}), capping", name, toCheck, min );
            return min;
        }

        if( toCheck.CompareTo(max) <= 0 )
            return toCheck;

        Logger.Warning( "{0} ({1}) > maximum ({2}), capping", name, toCheck, max );
        return max;
    }

    protected TileCoordinates? Cap( TileCoordinates toCheck )
    {
        if( !Initialized )
        {
            Logger.Error("Projection is not initialized");
            return null;
        }

        var xTile = Cap( toCheck.X, MinTile.X, MaxTile.X, "Tile X Coordinate" );
        var yTile = Cap( toCheck.Y, MinTile.Y, MaxTile.Y, "Tile Y Coordinate" );

        return new TileCoordinates( xTile, yTile );
    }

    public async Task<MemoryStream?> GetTileImageAsync(TileCoordinates coordinates)
    {
        if (!Initialized)
        {
            Logger.Error("Projection is not initialized");
            return null;
        }

        Logger.Verbose("Beginning image retrieval from web");

        if( !TryGetRequest(coordinates, out var request) )
        {
            Logger.Error( "Could not create HttpRequestMessage for tile ({0}, {1})", coordinates.X, coordinates.Y );
            return null;
        }

        var uriText = request!.RequestUri!.AbsoluteUri;
        var httpClient = new HttpClient();

        Logger.Verbose<string>("Querying {0}", uriText);

        HttpResponseMessage? response;

        try
        {
            response = await httpClient.SendAsync( request );
            Logger.Verbose<string>("Got response from {0}", uriText);
        }
        catch (Exception ex)
        {
            Logger.Error<Uri, string>("Image request from {0} failed, message was '{1}'",
                                                 request.RequestUri,
                                                 ex.Message);
            return null;
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            Logger.Error<string, HttpStatusCode, string>(
                "Image request from {0} failed with response code {1}, message was '{2}'",
                uriText,
                response.StatusCode,
                await response.Content.ReadAsStringAsync() );

            return null;
        }

        Logger.Verbose<string>("Reading response from {0}", uriText);

        return await ExtractImageDataAsync(response);
    }

    protected abstract bool TryGetRequest( TileCoordinates coordinates, out HttpRequestMessage? result );

    protected virtual async Task<MemoryStream?> ExtractImageDataAsync( HttpResponseMessage response )
    {
        try
        {
            await using var responseStream = await response.Content.ReadAsStreamAsync();

            var memStream = new MemoryStream();
            await responseStream.CopyToAsync( memStream );

            return memStream;
        }
        catch( Exception ex )
        {
            Logger.Error<Uri, string>( "Could not retrieve bitmap image stream from {0}, message was '{1}'",
                                       response.RequestMessage!.RequestUri!,
                                       ex.Message );

            return null;
        }
    }
}