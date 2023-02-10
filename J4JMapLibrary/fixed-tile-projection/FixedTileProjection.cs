using J4JSoftware.Logging;
using System.Numerics;

namespace J4JMapLibrary;

public abstract class FixedTileProjection<TScope, TAuth> : MapProjection<TScope, TAuth>, IFixedTileProjection<TScope>
    where TScope : TileScope, new()
    where TAuth : class
{
    protected FixedTileProjection(
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( mapServer, logger )
    {
        TileCache = tileCache;
        TileXRange = new MinMax<int>( 0, 0 );
        TileYRange = new MinMax<int>( 0, 0 );
    }

    protected FixedTileProjection(
        IProjectionCredentials credentials,
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( credentials, mapServer, logger )
    {
        TileCache = tileCache;
        TileXRange = new MinMax<int>( 0, 0 );
        TileYRange = new MinMax<int>( 0, 0 );
    }

    public int Height => Scope.YRange.Maximum - Scope.YRange.Minimum + 1;
    public int Width => Scope.XRange.Maximum - Scope.XRange.Minimum + 1;

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

    public async Task<List<IFixedMapTile>?> GetViewportRegionAsync(
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

    public async Task<FixedTileExtract?> GetViewportTilesAsync(
        Viewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    )
    {
        if( !Initialized )
        {
            Logger.Error( "Projection not initialized" );
            return null;
        }

        viewportData = viewportData.Constrain( Scope );

        var cartesianCenter = new Cartesian( Scope );
        cartesianCenter.SetCartesian(
            Scope.LatLongToCartesian( viewportData.CenterLatitude, viewportData.CenterLongitude ) );

        var corner1 = new Vector3( cartesianCenter.X - viewportData.Width / 2,
                                   cartesianCenter.Y + viewportData.Height / 2,
                                   0 );
        var corner2 = new Vector3( corner1.X + viewportData.Width, corner1.Y, 0 );
        var corner3 = new Vector3( corner2.X, corner2.Y - viewportData.Height, 0 );
        var corner4 = new Vector3( corner1.X, corner3.Y, 0 );

        var corners = new[] { corner1, corner2, corner3, corner4 };

        var vpCenter = new Vector3( cartesianCenter.X, cartesianCenter.Y, 0 );

        // apply rotation if one is defined
        // heading == 270 is rotation == 90, hence the angle adjustment
        if( viewportData.Heading != 0 )
        {
            corners = corners.ApplyTransform(
                Matrix4x4.CreateRotationZ( ( 360 - viewportData.Heading ) * MapConstants.RadiansPerDegree, vpCenter ) );
        }

        // find the range of tiles covering the mapped rectangle
        var minTileX = CartesianToTile( corners.Min( x => x.X ) );
        var maxTileX = CartesianToTile( corners.Max( x => x.X ) );

        // figuring out the min/max of y coordinates is a royal pain in the ass...
        // because in display space, increasing y values take you >>down<< the screen,
        // not up the screen. So the first adjustment is to subject the raw Y values from
        // the height of the projection to reverse the direction. 
        var minTileY = CartesianToTile( corners.Min( y => Height - y.Y ) );
        var maxTileY = CartesianToTile( corners.Max( y => Height - y.Y ) );

        minTileX = minTileX < 0 ? 0 : minTileX;
        minTileY = minTileY < 0 ? 0 : minTileY;

        var maxTiles = Height / MapServer.TileHeightWidth - 1;
        maxTileX = maxTileX > maxTiles ? maxTiles : maxTileX;
        maxTileY = maxTileY > maxTiles ? maxTiles : maxTileY;

        var retVal = new FixedTileExtract( this, Logger );

        for( var xTile = minTileX; xTile <= maxTileX; xTile++ )
        {
            for( var yTile = minTileY; yTile <= maxTileY; yTile++ )
            {
                var mapTile = await FixedMapTile.CreateAsync( this, xTile, yTile, ctx: ctx );

                if( !deferImageLoad )
                    await mapTile.GetImageAsync( ctx: ctx );

                if( !retVal.Add( (IMapTile<TScope>) mapTile ) )
                    Logger.Error( "Problem adding FixedMapTile to collection (probably differing ITileScope)" );
            }
        }

        return retVal;
    }

    private int CartesianToTile(float value) =>
        Convert.ToInt32(Math.Floor(value / MapServer.TileHeightWidth));

    // thanx to 3dGrabber for this
    // https://stackoverflow.com/questions/383587/how-do-you-do-integer-exponentiation-in-c
    public static int Pow( int numBase, int exp ) =>
        Enumerable
           .Repeat( numBase, Math.Abs( exp ) )
           .Aggregate( 1, ( a, b ) => exp < 0 ? a / b : a * b );

    // this assumes IMapServer has been set and scale is valid
    protected override void SetSizes( int scale )
    {
        var cellsInDimension = Pow( 2, scale );
        var projHeightWidth = MapServer.TileHeightWidth * cellsInDimension;

        Scope.XRange = new MinMax<int>( 0, projHeightWidth - 1 );
        Scope.YRange = new MinMax<int>( 0, projHeightWidth - 1 );
        TileXRange = new MinMax<int>( 0, cellsInDimension - 1 );
        TileYRange = new MinMax<int>( 0, cellsInDimension - 1 );
    }
}
