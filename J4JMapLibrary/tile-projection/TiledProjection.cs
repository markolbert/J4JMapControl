using J4JSoftware.Logging;
using System.Net;
using System.Runtime.CompilerServices;

namespace J4JMapLibrary;

public abstract partial class TiledProjection : MapProjection, ITiledProjection
{
    // thanx to Benjamin Hodgson, Ray Burns, Regent et al for this!
    // https://stackoverflow.com/questions/1664793/how-to-restrict-access-to-nested-class-member-to-enclosing-class
#pragma warning disable CS8618
    protected static Func<ITiledProjection, int, int, MapTile> CreateMapTileInternal;
    //protected static Func<ITiledProjection, IJ4JLogger, MapPoint> CreateMapPointInternal;
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
        IJ4JLogger logger
    )
    :base(srcConfig, logger)
    {
        // thanx to Benjamin Hodgson, Ray Burns, Regent et al for this!
        // https://stackoverflow.com/questions/1664793/how-to-restrict-access-to-nested-class-member-to-enclosing-class
        RuntimeHelpers.RunClassConstructor( typeof( MapTile ).TypeHandle );
        RuntimeHelpers.RunClassConstructor( typeof( TileImageStream ).TypeHandle );

        MinTile = CreateMapTileInternal( this, 0, 0 );
        MaxTile = CreateMapTileInternal(this, 0, 0);
    }

    public MapTile MinTile { get; protected set; }
    public MapTile MaxTile { get; protected set; }

    public int MaxScale { get; protected set; }
    public int MinScale { get; protected set; }

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

            _scale = MapExtensions.ConformValueToRange(value, MinScale, MaxScale, "Scale", Logger);
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

        MaxTile = CreateMapTileInternal(this, numCells - 1, numCells - 1);
    }

    public int TileHeightWidth { get; protected set; }

    public double GroundResolution( double latitude )
    {
        if( !Initialized )
        {
            Logger.Error( "Projection is not initialized" );
            return 0;
        }

        latitude = MapExtensions.ConformValueToRange( latitude, MinLatitude, MaxLatitude, "Latitude", Logger );

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

        var retVal = CreateMapTileInternal( this, xTile, yTile );

        return Cap( retVal )!;
    }

    public MapTile? CreateMapTileFromXY( int x, int y )
    {
        if (!Initialized)
        {
            Logger.Error("Not initialized");
            return null;
        }

        x = MapExtensions.ConformValueToRange( x, MinX, MaxX, "X coordinate", Logger );
        y = MapExtensions.ConformValueToRange( y, MinY, MaxY, "Y coordinate", Logger );

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

        var xTile = MapExtensions.ConformValueToRange( toCheck.X, MinTile.X, MaxTile.X, "Tile X Coordinate", Logger );
        var yTile = MapExtensions.ConformValueToRange( toCheck.Y, MinTile.Y, MaxTile.Y, "Tile Y Coordinate", Logger );

        return CreateMapTileInternal(this, xTile, yTile );
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