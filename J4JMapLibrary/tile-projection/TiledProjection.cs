using J4JSoftware.Logging;
using System.Drawing;
using System.Net;
using System.Runtime.CompilerServices;

namespace J4JMapLibrary;

public abstract partial class TiledProjection : MapProjection, ITiledProjection
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

        MinTile = new MapTile( this, 0, 0, Logger );
        MaxTile = new MapTile( this, 0, 0, Logger );
    }

    protected TiledProjection(
        ILibraryConfiguration libConfiguration,
        bool canBeCached,
        IJ4JLogger logger
    )
        : base( libConfiguration, logger )
    {
        CanBeCached = canBeCached;

        MinTile = new MapTile( this, 0, 0, Logger );
        MaxTile = new MapTile( this, 0, 0, Logger );
    }

    public bool CanBeCached { get; }

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

            _scale = InternalExtensions.ConformValueToRange(value, MinScale, MaxScale, "Scale", Logger);
            SetSizes(_scale - MinScale);
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

        MaxTile = new MapTile(this, numCells - 1, numCells - 1, Logger);
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

    protected MapTile? Cap( MapTile toCheck )
    {
        if( !Initialized )
        {
            Logger.Error("Projection is not initialized");
            return null;
        }

        var xTile = InternalExtensions.ConformValueToRange( toCheck.X, MinTile.X, MaxTile.X, "Tile X Coordinate", Logger );
        var yTile = InternalExtensions.ConformValueToRange( toCheck.Y, MinTile.Y, MaxTile.Y, "Tile Y Coordinate", Logger );

        return new MapTile(this, xTile, yTile, Logger );
    }

    public LatLong CartesianToLatLong(int x, int y)
    {
        var retVal = new LatLong(this, Logger);

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
        var retVal = new Cartesian(this, Logger);

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

    public abstract bool TryGetRequest( MapTile tile, out HttpRequestMessage? result );

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