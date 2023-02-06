using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class TiledProjection<TScope> : MapProjection<TScope>, ITiledProjection<TScope>
    where TScope : TiledMapScope, new()
{
    // thanx to 3dGrabber for this
    // https://stackoverflow.com/questions/383587/how-do-you-do-integer-exponentiation-in-c
    public static int Pow(int numBase, int exp) =>
        Enumerable
           .Repeat(numBase, Math.Abs(exp))
           .Aggregate(1, (a, b) => exp < 0 ? a / b : a * b);

    public event EventHandler<int>? ScaleChanged; 

    protected TiledProjection(
        ISourceConfiguration srcConfig,
        IJ4JLogger logger, 
        ITileCache? tileCache = null
    )
    :base(srcConfig, logger)
    {
        TileCache = tileCache;
        TileXRange = new MinMax<int>( 0, 0 );
        TileYRange = new MinMax<int>(0, 0);
    }

    protected TiledProjection(
        ILibraryConfiguration libConfiguration,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( libConfiguration, logger )
    {
        TileCache = tileCache;
        TileXRange = new MinMax<int>(0, 0);
        TileYRange = new MinMax<int>(0, 0);
    }

    public int Height => Scope.YRange.Maximum - Scope.YRange.Minimum + 1;
    public int Width => Scope.XRange.Maximum - Scope.XRange.Minimum + 1;

    public ITileCache? TileCache { get; }

    public void SetScale( int scale )
    {
        if( !Initialized )
        {
            Logger.Error( "Trying to set scale before projection is initialized, ignoring" );
            return;
        }

        Scope.Scale = Scope.ScaleRange.ConformValueToRange( scale, "Scale" );

        SetSizes( scale );

        ScaleChanged?.Invoke( this, scale );
    }

    // this assumes TileHeightWidth has been set and scale is valid
    protected void SetSizes( int scale )
    {
        var cellsInDimension = Pow( 2, scale );
        var projHeightWidth = TileHeightWidth * cellsInDimension;

        Scope.XRange = new MinMax<int>( 0, projHeightWidth - 1 );
        Scope.YRange = new MinMax<int>( 0, projHeightWidth - 1 );
        TileXRange = new MinMax<int>( 0, cellsInDimension - 1 );
        TileYRange = new MinMax<int>( 0, cellsInDimension - 1 );
    }

    public MinMax<int> TileXRange { get; private set; }
    public MinMax<int> TileYRange { get; private set; }

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

        latitude = Scope.LatitudeRange.ConformValueToRange( latitude, "Latitude" );

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