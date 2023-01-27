using J4JSoftware.Logging;
using System.Net;
using System.Runtime.CompilerServices;

namespace J4JMapLibrary;

public abstract partial class TiledProjection : MapProjection, ITiledProjection
{
    // thanx to Benjamin Hodgson, Ray Burns, Regent et al for this!
    // https://stackoverflow.com/questions/1664793/how-to-restrict-access-to-nested-class-member-to-enclosing-class
#pragma warning disable CS8618
    protected static Func<ITiledProjection, IJ4JLogger, LatLong> CreateLatLongInternal;
    protected static Func<ITiledProjection, IJ4JLogger, Cartesian> CreateCartesianInternal;
    protected static Func<ITiledProjection, IJ4JLogger, MapPoint> CreateMapPointInternal;
    protected static Func<ITiledProjection, int, int, IJ4JLogger, MapTile> MapTileFromCoordinates;
    protected static Func<MapPoint, IJ4JLogger, MapTile> MapTileFromMapPoint;
    protected static Func<LatLong, IJ4JLogger, MapTile> MapTileFromLatLong;
    protected static Func<ITiledProjection, MapTile, MemoryStream?, TileImageStream> CreateTileImageStream;
#pragma warning restore CS8618

    // thanx to 3dGrabber for this
    // https://stackoverflow.com/questions/383587/how-do-you-do-integer-exponentiation-in-c
    public static int Pow(int numBase, int exp) =>
        Enumerable
           .Repeat(numBase, Math.Abs(exp))
           .Aggregate(1, (a, b) => exp < 0 ? a / b : a * b);

    private int _scale;

    protected TiledProjection(
        ISourceConfiguration srcConfig,
        bool canBeCached,
        IJ4JLogger logger
    )
    :base(srcConfig, logger)
    {
        ExecuteRuntimeHelpers();

        CanBeCached = canBeCached;

        MinTile = MapTileFromCoordinates( this, 0, 0, Logger );
        MaxTile = MapTileFromCoordinates(this, 0, 0, Logger);
    }

    protected TiledProjection(
        ILibraryConfiguration libConfiguration,
        bool canBeCached,
        IJ4JLogger logger
    )
        : base( libConfiguration, logger )
    {
        ExecuteRuntimeHelpers();

        CanBeCached = canBeCached;

        MinTile = MapTileFromCoordinates(this, 0, 0, Logger);
        MaxTile = MapTileFromCoordinates(this, 0, 0, Logger);
    }

    private void ExecuteRuntimeHelpers()
    {
        // thanx to Benjamin Hodgson, Ray Burns, Regent et al for this!
        // https://stackoverflow.com/questions/1664793/how-to-restrict-access-to-nested-class-member-to-enclosing-class
        RuntimeHelpers.RunClassConstructor(typeof(Cartesian).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(LatLong).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(MapTile).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(TileImageStream).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(MapPoint).TypeHandle);
    }

    public bool CanBeCached { get; }

    public MapTile MinTile { get; protected set; }
    public MapTile MaxTile { get; protected set; }

    public int MaxScale { get; protected set; }
    public int MinScale { get; protected set; }

    protected List<MapPoint> RegisteredPoints { get; } = new();

    public virtual MapPoint CreateMapPoint()
    {
        var retVal = CreateMapPointInternal(this, Logger);
        RegisteredPoints.Add(retVal);

        return retVal;
    }

    public virtual int Scale
    {
        get => _scale;

        set
        {
            if (!Initialized)
            {
                Logger.Error("Trying to set scale before projection is initialized, ignoring");
                return;
            }

            _scale = InternalExtensions.ConformValueToRange(value, MinScale, MaxScale, "Scale", Logger);
            SetSizes(_scale - MinScale);

            foreach (var point in RegisteredPoints)
            {
                point.UpdateCartesian();
            }
        }
    }

    // this assumes *scale* has been normalized (i.e., x -> x - MinScale)
    // and TileHeightWidth has been set
    protected void SetSizes(int scale)
    {
        var numCells = Pow(2, scale);
        var heightWidth = TileHeightWidth * numCells;

        MinX = 0;
        MaxX = heightWidth - 1;
        MinY = 0;
        MaxY = heightWidth - 1;

        MaxTile = MapTileFromCoordinates(this, numCells - 1, numCells - 1, Logger);
    }

    public int TileHeightWidth { get; protected set; }

    public double GroundResolution( double latitude )
    {
        if( !Initialized )
        {
            Logger.Error( "Projection is not initialized" );
            return 0;
        }

        latitude = InternalExtensions.ConformValueToRange( latitude, MinLatitude, MaxLatitude, "Latitude", Logger );

        return Math.Cos( latitude * MapConstants.RadiansPerDegree )
          * MapConstants.EarthCircumferenceMeters
          / ( MaxX - MinX );
    }

    public string MapScale( double latitude, double dotsPerInch ) =>
        $"1 : {GroundResolution( latitude ) * dotsPerInch / MapConstants.MetersPerInch}";

    public virtual MapTile? CreateMapTile( int xTile, int yTile )
    {
        if( !Initialized )
        {
            Logger.Error("Projection is not initialized");
            return null;
        }

        var retVal = MapTileFromCoordinates( this, xTile, yTile, Logger);

        return Cap( retVal )!;
    }

    public MapTile? CreateMapTileFromXY( int x, int y )
    {
        if (!Initialized)
        {
            Logger.Error("Not initialized");
            return null;
        }

        x = InternalExtensions.ConformValueToRange( x, MinX, MaxX, "X coordinate", Logger );
        y = InternalExtensions.ConformValueToRange( y, MinY, MaxY, "Y coordinate", Logger );

        return CreateMapTile(Convert.ToInt32( Math.Floor( x / 256.0 ) ),
                                    Convert.ToInt32( Math.Floor( y / 256.0 ) ) );
    }

    protected MapTile? Cap( MapTile toCheck )
    {
        if( !Initialized )
        {
            Logger.Error("Projection is not initialized");
            return null;
        }

        var xTile = InternalExtensions.ConformValueToRange( toCheck.X, MinTile.X, MaxTile.X, "Tile X Coordinate", Logger );
        var yTile = InternalExtensions.ConformValueToRange( toCheck.Y, MinTile.Y, MaxTile.Y, "Tile Y Coordinate", Logger );

        return MapTileFromCoordinates(this, xTile, yTile, Logger );
    }

    public async Task<TileImageStream> GetTileImageAsync( MapTile tile )
    {
        if( !Initialized )
        {
            Logger.Error( "Projection is not initialized" );
            return CreateTileImageStream( this, tile, null );
        }

        tile = Cap( tile )!;

        Logger.Verbose( "Beginning image retrieval from web" );

        if( !TryGetRequest( tile, out var request ) )
        {
            Logger.Error( "Could not create HttpRequestMessage for tile ({0}, {1})", tile.X, tile.Y );
            return CreateTileImageStream(this, tile, null);
        }

        var uriText = request!.RequestUri!.AbsoluteUri;
        var httpClient = new HttpClient();

        Logger.Verbose<string>( "Querying {0}", uriText );

        HttpResponseMessage? response;

        try
        {
            response = await httpClient.SendAsync( request );
            Logger.Verbose<string>( "Got response from {0}", uriText );
        }
        catch( Exception ex )
        {
            Logger.Error<Uri, string>( "Image request from {0} failed, message was '{1}'",
                                       request.RequestUri,
                                       ex.Message );
            return CreateTileImageStream(this, tile, null);
        }

        if ( response.StatusCode != HttpStatusCode.OK )
        {
            Logger.Error<string, HttpStatusCode, string>(
                "Image request from {0} failed with response code {1}, message was '{2}'",
                uriText,
                response.StatusCode,
                await response.Content.ReadAsStringAsync() );

            return CreateTileImageStream(this, tile, null);
        }

        Logger.Verbose<string>( "Reading response from {0}", uriText );

        return CreateTileImageStream( this, tile, await ExtractImageDataAsync( response ) );
    }

    public async IAsyncEnumerable<TileImageStream> GetTileImagesAsync( IEnumerable<MapTile> tiles )
    {
        foreach( var curTile in tiles.Distinct( MapTile.DefaultComparer ) )
        {
            yield return await GetTileImageAsync( curTile );
        }
    }

    public LatLong CartesianToLatLong(int x, int y)
    {
        var retVal = CreateLatLongInternal(this, Logger);

        if (!Initialized)
        {
            Logger.Error("Not initialized");
            return retVal;
        }

        x = InternalExtensions.ConformValueToRange(x, MinX, MaxX, "X coordinate", Logger);
        y = InternalExtensions.ConformValueToRange(y, MinY, MaxY, "Y coordinate", Logger);

        retVal.Latitude = (2 * Math.Atan(Math.Exp(MapConstants.TwoPi * y / Height)) - MapConstants.HalfPi)
          / MapConstants.RadiansPerDegree;
        retVal.Longitude = 360 * x / Width - 180;

        return retVal;
    }

    public Cartesian LatLongToCartesian(double latitude, double longitude)
    {
        var retVal = CreateCartesianInternal(this, Logger);

        if (!Initialized)
        {
            Logger.Error("Not initialized");
            return retVal;
        }

        latitude = InternalExtensions.ConformValueToRange(latitude, MinLatitude, MaxLatitude, "Latitude", Logger);
        longitude = InternalExtensions.ConformValueToRange(longitude, MinLongitude, MaxLongitude, "Longitude", Logger);

        var x = Width * (longitude / 360 + 0.5);
        var y = Width * Math.Log(Math.Tan(MapConstants.QuarterPi + latitude * MapConstants.RadiansPerDegree / 2)) / MapConstants.TwoPi;

        try
        {
            retVal.X = Convert.ToInt32(Math.Round(x));
            retVal.Y = Convert.ToInt32(Math.Round(y));
        }
        catch (Exception ex)
        {
            Logger.Error<string>("Could not convert double to int32, message was '{0}'", ex.Message);
        }

        return retVal;
    }

    protected abstract bool TryGetRequest( MapTile tile, out HttpRequestMessage? result );

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