using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class TiledProjection : MapProjection, ITiledProjection
{
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
        CanBeCached = canBeCached;
    }

    protected TiledProjection(
        ILibraryConfiguration libConfiguration,
        bool canBeCached,
        IJ4JLogger logger
    )
        : base( libConfiguration, logger )
    {
        CanBeCached = canBeCached;
    }

    public bool CanBeCached { get; }

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

            _scale = InternalExtensions.ConformValueToRange(value, Metrics.ScaleRange, "Scale");
            SetSizes( _scale - Metrics.ScaleRange.Minimum );
        }
    }

    // this assumes *scale* has been normalized (i.e., x -> x - MinScale)
    // and TileHeightWidth has been set
    protected void SetSizes(int scale)
    {
        var numCells = Pow(2, scale);
        var heightWidth = TileHeightWidth * numCells;

        Metrics = Metrics with
        {
            XRange = new MinMax<int>( 0, heightWidth - 1 ),
            YRange = new MinMax<int>( 0, heightWidth - 1 ),
            TileXRange = new MinMax<int>( 0, numCells - 1 ),
            TileYRange = new MinMax<int>( 0, numCells - 1 )
        };
    }

    public int TileHeightWidth { get; protected set; }

    public double GroundResolution( double latitude )
    {
        if( !Initialized )
        {
            Logger.Error( "Projection is not initialized" );
            return 0;
        }

        latitude = InternalExtensions.ConformValueToRange( latitude, Metrics.LatitudeRange, "Latitude" );

        return Math.Cos( latitude * MapConstants.RadiansPerDegree )
          * MapConstants.EarthCircumferenceMeters
          / Width;
    }

    public string MapScale( double latitude, double dotsPerInch ) =>
        $"1 : {GroundResolution( latitude ) * dotsPerInch / MapConstants.MetersPerInch}";

    protected MapTile? Cap( MapTile toCheck )
    {
        if( !Initialized )
        {
            Logger.Error("Projection is not initialized");
            return null;
        }

        var xTile = InternalExtensions.ConformValueToRange( toCheck.X, Metrics.TileXRange, "Tile X Coordinate" );
        var yTile = InternalExtensions.ConformValueToRange( toCheck.Y, Metrics.TileYRange, "Tile Y Coordinate" );

        return new MapTile(this, xTile, yTile );
    }

    public abstract HttpRequestMessage? GetRequest( MapTile tile  );

    public virtual async Task<MemoryStream?> ExtractImageDataAsync( HttpResponseMessage response )
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