using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class VariableTileProjection<TScope, TAuth> : MapProjection<TScope, TAuth>
    where TScope : MapScope, new()
    where TAuth : class
{
    // thanx to 3dGrabber for this
    // https://stackoverflow.com/questions/383587/how-do-you-do-integer-exponentiation-in-c
    public static int Pow(int numBase, int exp) =>
        Enumerable
           .Repeat(numBase, Math.Abs(exp))
           .Aggregate(1, (a, b) => exp < 0 ? a / b : a * b);

    protected VariableTileProjection(
        ISourceConfiguration srcConfig,
        IMapServer mapServer,
        IJ4JLogger logger
    )
    :base(srcConfig, mapServer, logger)
    {
    }

    protected VariableTileProjection(
        ILibraryConfiguration libConfiguration,
        IMapServer mapServer,
        IJ4JLogger logger
    )
        : base( libConfiguration, mapServer, logger )
    {
    }

    public int Height { get; private set; }
    public int Width { get; private set; }

    // this assumes TileHeightWidth has been set and scale is valid
    protected override void SetSizes(int scale)
    {
        var cellsInDimension = Pow(2, scale);
        Height = TileHeightWidth * cellsInDimension;
        Width = Height;
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

        latitude = Scope.LatitudeRange.ConformValueToRange( latitude, "Latitude" );

        return (float) Math.Cos( latitude * MapConstants.RadiansPerDegree )
          * MapConstants.EarthCircumferenceMeters
          / Width;
    }

    public string MapScale( float latitude, float dotsPerInch ) =>
        $"1 : {GroundResolution( latitude ) * dotsPerInch / MapConstants.MetersPerInch}";
}