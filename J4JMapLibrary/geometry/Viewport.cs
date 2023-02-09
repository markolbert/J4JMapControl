using System.Numerics;
using J4JSoftware.Logging;

// this class has to be in a different namespace from the
// assembly's main one so that we can avoid naming conflicts between
// property names and extension method names
namespace J4JMapLibrary.Viewport;

public class Viewport
{
    private static readonly MinMax<float> NonNegativeRange = new( 0F, float.MaxValue );

    private readonly IJ4JLogger _logger;

    private IFixedTileProjection? _projection;
    private float _height;
    private float _width;
    private float _centerLat;
    private float _centerLong;
    private float _heading;

    public Viewport(
        IJ4JLogger logger
    )
    {
        _logger = logger;
        _logger.SetLoggedType( GetType() );
    }

    private void Projection_ScaleChanged( object? sender, int e )
    {
        UpdateNeeded = true;
    }

    public bool UpdateNeeded { get; private set; } = true;

    public IFixedTileProjection Projection
    {
        get
        {
            if( _projection != null )
                return _projection;

            var mesg = $"{nameof( Projection )} is not defined";
            _logger.Fatal( mesg );
            throw new NullReferenceException( mesg );
        }

        internal set
        {
            _projection = value;
            _projection.ScaleChanged += Projection_ScaleChanged;

            Scope = FixedTileScope.Copy( (FixedTileScope) _projection.GetScope() );

            UpdateNeeded = true;
        }
    }

    public FixedTileScope Scope { get; private set; } = new();

    public float CenterLatitude
    {
        get => _centerLat;

        internal set
        {
            _centerLat = Scope.LatitudeRange.ConformValueToRange( value, "Latitude" );
            UpdateNeeded = true;
        }
    }

    public float CenterLongitude
    {
        get => _centerLong;

        internal set
        {
            _centerLong = Scope.LongitudeRange.ConformValueToRange( value, "Longitude" );
            UpdateNeeded = true;
        }
    }

    public float Height
    {
        get => _height;

        internal set
        {
            _height = NonNegativeRange.ConformValueToRange( value, "Height" );
            UpdateNeeded = true;
        }
    }

    public float Width
    {
        get => _width;

        internal set
        {
            _width = NonNegativeRange.ConformValueToRange( value, "Width" );
            UpdateNeeded = true;
        }
    }

    // in degrees; north is 0/360; stored as mod 360
    public float Heading
    {
        get => _heading;

        internal set
        {
            _heading = value % 360;
            UpdateNeeded = true;
        }
    }

    public async Task<List<FixedMapTile>?> GetViewportRegionAsync(
        bool deferImageLoad = false,
        CancellationToken ctx = default
    )
    {
        var tileList = await GetViewportTilesAsync( ctx );

        if( tileList == null )
            return null;

        return await tileList.GetTilesAsync( Projection, ctx )
                             .ToListAsync( ctx );
    }

    public async Task<MapTileList?> GetViewportTilesAsync(
        CancellationToken ctx,
        bool deferImageLoad = false
    )
    {
        if( Height == 0 || Width == 0 )
        {
            _logger.Error( "Viewport not defined" );
            return null;
        }

        if( _projection == null )
        {
            _logger.Error( "Projection not defined" );
            return null;
        }

        var cartesianCenter = new Cartesian( Scope );
        cartesianCenter.SetCartesian( Scope.LatLongToCartesian( CenterLatitude, CenterLongitude ) );

        var corner1 = new Vector3( cartesianCenter.X - Width / 2, cartesianCenter.Y + Height / 2, 0 );
        var corner2 = new Vector3( corner1.X + Width, corner1.Y, 0 );
        var corner3 = new Vector3( corner2.X, corner2.Y - Height, 0 );
        var corner4 = new Vector3( corner1.X, corner3.Y, 0 );

        var corners = new[] { corner1, corner2, corner3, corner4 };

        var vpCenter = new Vector3( cartesianCenter.X, cartesianCenter.Y, 0 );

        // apply rotation if one is defined
        // heading == 270 is rotation == 90, hence the angle adjustment
        if( _heading != 0 )
        {
            corners = corners.ApplyTransform(
                Matrix4x4.CreateRotationZ( ( 360 - _heading ) * MapConstants.RadiansPerDegree, vpCenter ) );
        }

        // find the range of tiles covering the mapped rectangle
        var minTileX = CartesianToTile( corners.Min( x => x.X ) );
        var maxTileX = CartesianToTile( corners.Max( x => x.X ) );

        // figuring out the min/max of y coordinates is a royal pain in the ass...
        // because in display space, increasing y values take you >>down<< the screen,
        // not up the screen. So the first adjustment is to subject the raw Y values from
        // the height of the projection to reverse the direction. 
        var minTileY = CartesianToTile( corners.Min( y => Projection.Height - y.Y ) );
        var maxTileY = CartesianToTile( corners.Max( y => Projection.Height - y.Y ) );

        minTileX = minTileX < 0 ? 0 : minTileX;
        minTileY = minTileY < 0 ? 0 : minTileY;

        var maxTiles = Projection.Height / Projection.MapServer.TileHeightWidth - 1;
        maxTileX = maxTileX > maxTiles ? maxTiles : maxTileX;
        maxTileY = maxTileY > maxTiles ? maxTiles : maxTileY;

        var retVal = new MapTileList();

        for( var xTile = minTileX; xTile <= maxTileX; xTile++ )
        {
            for( var yTile = minTileY; yTile <= maxTileY; yTile++ )
            {
                var mapTile = await FixedMapTile.CreateAsync( Projection, xTile, yTile, ctx: ctx );

                if( !deferImageLoad )
                    await mapTile.GetImageAsync( ctx: ctx );

                if( !retVal.Add( mapTile ) )
                    _logger.Error( "Problem adding FixedMapTile to collection (probably differing IFixedTileScope)" );
            }
        }

        return retVal;
    }

    private int CartesianToTile( float value ) =>
        Convert.ToInt32( Math.Floor( value / Projection.MapServer.TileHeightWidth ) );
}
