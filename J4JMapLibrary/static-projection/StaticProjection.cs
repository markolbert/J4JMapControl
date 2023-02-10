using J4JSoftware.Logging;
using System.Numerics;

namespace J4JMapLibrary;

public abstract class StaticProjection<TScope, TAuth> : MapProjection<TScope, TAuth>, IStaticProjection
    where TScope : MapScope, new()
    where TAuth : class
{
    protected StaticProjection(
        IMapServer mapServer,
        IJ4JLogger logger
    )
        : base( mapServer, logger )
    {
    }

    protected StaticProjection(
        IProjectionCredentials credentials,
        IMapServer mapServer,
        IJ4JLogger logger
    )
        : base( credentials, mapServer, logger )
    {
    }

    public int Height { get; private set; }
    public int Width { get; private set; }

    public int TileHeightWidth { get; protected set; }

    public string ImageFileExtension { get; private set; } = string.Empty;

    // thanx to 3dGrabber for this
    // https://stackoverflow.com/questions/383587/how-do-you-do-integer-exponentiation-in-c
    public static int Pow( int numBase, int exp ) =>
        Enumerable
           .Repeat( numBase, Math.Abs( exp ) )
           .Aggregate( 1, ( a, b ) => exp < 0 ? a / b : a * b );

    // this assumes TileHeightWidth has been set and scale is valid
    protected override void SetSizes( int scale )
    {
        var cellsInDimension = Pow( 2, scale );
        Height = TileHeightWidth * cellsInDimension;
        Width = Height;
    }

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

    public async Task<List<IStaticFragment>?> GetViewportRegionAsync(
        Viewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    )
    {
        var extract = await GetViewportTilesAsync(viewportData, deferImageLoad, ctx);

        if (extract == null)
            return null;

        return await extract.GetTilesAsync(ctx)
                            .ToListAsync(ctx);
    }

    public async Task<StaticExtract?> GetViewportTilesAsync(
        Viewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    )
    {
        if (!Initialized)
        {
            Logger.Error("Projection not initialized");
            return null;
        }

        viewportData = viewportData.Constrain(Scope);

        var mapTile = new StaticFragment(this,
                                               viewportData.CenterLatitude,
                                               viewportData.CenterLongitude,
                                               viewportData.Height,
                                               viewportData.Width,
                                               Scope.Scale);

        // need to put in logic to rotate/heading

        if (!deferImageLoad)
            await mapTile.GetImageAsync(ctx: ctx);

        var retVal = new StaticExtract(this, Logger);

        if (!retVal.Add(mapTile))
            Logger.Error("Problem adding StaticFragment to collection (probably differing ITiledScope)");

        return retVal;
    }

}
