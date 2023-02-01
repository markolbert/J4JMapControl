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

    public event EventHandler<int>? ScaleChanged; 

    private int _scale;

    protected TiledProjection(
        ISourceConfiguration srcConfig,
        IJ4JLogger logger, 
        ITileCache? tileCache = null
    )
    :base(srcConfig, logger)
    {
        TileCache = tileCache;
    }

    protected TiledProjection(
        ILibraryConfiguration libConfiguration,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( libConfiguration, logger )
    {
        TileCache = tileCache;
    }

    public ITileCache? TileCache { get; }

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

            _scale = InternalExtensions.ConformValueToRange( value, Metrics.ScaleRange, "Scale" );

            SetSizes( _scale  );

            ScaleChanged?.Invoke( this, _scale );
        }
    }

    // this assumes TileHeightWidth has been set
    protected void SetSizes( int scale )
    {
        var cellsInDimension = Pow( 2, scale );
        var projHeightWidth = TileHeightWidth * cellsInDimension;

        Metrics = Metrics with
        {
            XRange = new MinMax<int>( 0, projHeightWidth - 1 ),
            YRange = new MinMax<int>( 0, projHeightWidth - 1 ),
            TileXRange = new MinMax<int>( 0, cellsInDimension - 1 ),
            TileYRange = new MinMax<int>( 0, cellsInDimension - 1 ),
            Scale = Scale
        };
    }

    public int TileHeightWidth { get; protected set; }
    public string ImageFileExtension { get; private set; } = string.Empty;

    protected void SetImageFileExtension( string urlText )
    {
        try
        {
            var imageUrl = new Uri( urlText );
            ImageFileExtension = Path.GetExtension( imageUrl.LocalPath );
        }
        catch( Exception ex )
        {
            Logger.Error<string>( "Could not determine image file extension, message was '{0}'", ex.Message );
        }
    }

    public float GroundResolution( float latitude )
    {
        if( !Initialized )
        {
            Logger.Error( "Projection is not initialized" );
            return 0;
        }

        latitude = InternalExtensions.ConformValueToRange( latitude, Metrics.LatitudeRange, "Latitude" );

        return (float) Math.Cos( latitude * MapConstants.RadiansPerDegree )
          * MapConstants.EarthCircumferenceMeters
          / Width;
    }

    public string MapScale( float latitude, float dotsPerInch ) =>
        $"1 : {GroundResolution( latitude ) * dotsPerInch / MapConstants.MetersPerInch}";

    public abstract HttpRequestMessage? GetRequest( MapTile tile  );

    public virtual async Task<byte[]?> ExtractImageDataAsync( HttpResponseMessage response )
    {
        try
        {
            await using var responseStream = await response.Content.ReadAsStreamAsync();

            var memStream = new MemoryStream();
            await responseStream.CopyToAsync( memStream );

            return memStream.ToArray();
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