using J4JSoftware.Logging;
using System.Numerics;

namespace J4JMapLibrary;

public abstract class TiledProjection<TAuth> : Projection<TAuth>, ITiledProjection
    where TAuth : class
{
    protected TiledProjection(
        IMapServer mapServer,
        TiledScale tiledScale,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( mapServer, tiledScale, logger )
    {
        TileCache = tileCache;
        TileXRange = new MinMax<int>( 0, 0 );
        TileYRange = new MinMax<int>( 0, 0 );
    }

    protected TiledProjection(
        IProjectionCredentials credentials,
        IMapServer mapServer,
        TiledScale tiledScale,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( credentials, mapServer, tiledScale, logger )
    {
        TileCache = tileCache;
        TileXRange = new MinMax<int>( 0, 0 );
        TileYRange = new MinMax<int>( 0, 0 );
    }

    public ITiledScale TiledScale => (ITiledScale) base.MapScale;

    public int Height => TiledScale.YRange.Maximum - TiledScale.YRange.Minimum + 1;
    public int Width => TiledScale.XRange.Maximum - TiledScale.XRange.Minimum + 1;

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

        latitude = MapServer.LatitudeRange.ConformValueToRange( latitude, "Latitude" );

        return (float) Math.Cos( latitude * MapConstants.RadiansPerDegree )
          * MapConstants.EarthCircumferenceMeters
          / Width;
    }

    public string ScaleDescription( float latitude, float dotsPerInch ) =>
        $"1 : {GroundResolution( latitude ) * dotsPerInch / MapConstants.MetersPerInch}";

    public async Task<List<ITiledFragment>?> GetViewportRegionAsync(
        Viewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    )
    {
        var extract = await GetViewportTilesAsync(viewportData, deferImageLoad, ctx);

        if (extract == null)
            return null;

        return await extract.GetTilesAsync(viewportData.Scale, ctx)
                            .ToListAsync(ctx);
    }

    public async Task<TiledExtract?> GetViewportTilesAsync(
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

        var cartesianCenter = new Cartesian( TiledScale );
        cartesianCenter.SetCartesian(
            TiledScale.LatLongToCartesian( viewportData.CenterLatitude, viewportData.CenterLongitude ) );

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

        var retVal = new TiledExtract( this, Logger );

        for( var xTile = minTileX; xTile <= maxTileX; xTile++ )
        {
            for( var yTile = minTileY; yTile <= maxTileY; yTile++ )
            {
                var mapTile = await TiledFragment.CreateAsync( this, xTile, yTile, viewportData.Scale, ctx: ctx );

                if( !deferImageLoad )
                    await mapTile.GetImageAsync( viewportData.Scale, ctx: ctx );

                if( !retVal.Add( mapTile ) )
                    Logger.Error( "Problem adding TiledFragment to collection (probably differing ITiledScale)" );
            }
        }

        return retVal;
    }

    private int CartesianToTile(float value) =>
        Convert.ToInt32(Math.Floor(value / MapServer.TileHeightWidth));
}
