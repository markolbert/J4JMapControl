using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class FixedTileProjection<TScope, TAuth> : MapProjection<TScope, TAuth>, IFixedTileProjection<TScope>
    where TScope : FixedTileScope, new()
    where TAuth : class
{
    // thanx to 3dGrabber for this
    // https://stackoverflow.com/questions/383587/how-do-you-do-integer-exponentiation-in-c
    public static int Pow(int numBase, int exp) =>
        Enumerable
           .Repeat(numBase, Math.Abs(exp))
           .Aggregate(1, (a, b) => exp < 0 ? a / b : a * b);

    protected FixedTileProjection(
        ISourceConfiguration srcConfig,
        IMapServer mapServer,
        IJ4JLogger logger, 
        ITileCache? tileCache = null
    )
    :base(srcConfig, mapServer, logger)
    {
        TileCache = tileCache;
        TileXRange = new MinMax<int>( 0, 0 );
        TileYRange = new MinMax<int>(0, 0);
    }

    protected FixedTileProjection(
        ILibraryConfiguration libConfiguration,
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( libConfiguration, mapServer, logger )
    {
        TileCache = tileCache;
        TileXRange = new MinMax<int>(0, 0);
        TileYRange = new MinMax<int>(0, 0);
    }

    public int Height => Scope.YRange.Maximum - Scope.YRange.Minimum + 1;
    public int Width => Scope.XRange.Maximum - Scope.XRange.Minimum + 1;

    // this assumes IMapServer has been set and scale is valid
    protected override void SetSizes(int scale)
    {
        var cellsInDimension = Pow(2, scale);
        var projHeightWidth = MapServer.TileHeightWidth * cellsInDimension;

        Scope.XRange = new MinMax<int>(0, projHeightWidth - 1);
        Scope.YRange = new MinMax<int>(0, projHeightWidth - 1);
        TileXRange = new MinMax<int>(0, cellsInDimension - 1);
        TileYRange = new MinMax<int>(0, cellsInDimension - 1);
    }

    public ITileCache? TileCache { get; }

    public MinMax<int> TileXRange { get; private set; }
    public MinMax<int> TileYRange { get; private set; }

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
}