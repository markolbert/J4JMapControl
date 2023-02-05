using System.Numerics;
using J4JSoftware.Logging;

namespace J4JMapLibrary.Viewport;

public class ViewportRectangle
{
    private static readonly MinMax<float> NonNegativeRange = new( 0F, float.MaxValue );

    private readonly IJ4JLogger _logger;

    private ITiledProjection? _projection;
    private float _height;
    private float _width;
    private float _centerLat;
    private float _centerLong;
    private float _heading;

    public ViewportRectangle(
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

    public ITiledProjection Projection
    {
        get
        {
            if( _projection != null )
                return _projection;

            var mesg = $"{nameof( Projection )} is not defined";
            _logger.Fatal(mesg);
            throw new NullReferenceException( mesg );
        }

        internal set
        {
            _projection = value;
            _projection.ScaleChanged += Projection_ScaleChanged;

            Scope = TiledMapScope.Copy( (TiledMapScope) _projection.GetScope() );
            
            UpdateNeeded = true;
        }
    }

    public TiledMapScope Scope { get; private set; } = new();

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
            _centerLong = Scope.LongitudeRange.ConformValueToRange(value, "Longitude");
            UpdateNeeded = true;
        }
    }

    public float Height
    {
        get => _height;

        internal set
        {
            _height = NonNegativeRange.ConformValueToRange(value, "Height");
            UpdateNeeded = true;
        }
    }

    public float Width
    {
        get => _width;

        internal set
        {
            _width = NonNegativeRange.ConformValueToRange(value, "Width");
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

    public async Task<List<MapTile>?> GetViewportRegionAsync(
        CancellationToken cancellationToken,
        bool deferImageLoad = false
    )
    {
        var tileList = await GetViewportTilesAsync( cancellationToken );

        if( tileList == null )
            return null;

        return await tileList.GetTilesAsync( Projection, cancellationToken )
                             .ToListAsync( cancellationToken );
    }

    public async Task<MapTileList?> GetViewportTilesAsync(
        CancellationToken cancellationToken,
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

        var cartesianCenter = new Cartesian(Scope);
        cartesianCenter.SetCartesian(Scope.LatLongToCartesian(CenterLatitude, CenterLongitude));

        var corner1 = new Vector3(cartesianCenter.X - Width / 2, cartesianCenter.Y + Height / 2, 0);
        var corner2 = new Vector3(corner1.X + Width, corner1.Y, 0);
        var corner3 = new Vector3(corner2.X, corner2.Y - Height, 0);
        var corner4 = new Vector3(corner1.X, corner3.Y, 0);

        var corners = new[]
        {
            corner1,
            corner2,
            corner3,
            corner4
        };

        var vpCenter = new Vector3( cartesianCenter.X, cartesianCenter.Y, 0 );

        // apply rotation if one is defined
        // heading == 270 is rotation == 90, hence the angle adjustment
        if( _heading != 0 )
            corners = corners.ApplyTransform(
                Matrix4x4.CreateRotationZ( ( 360 - _heading ) * MapConstants.RadiansPerDegree, vpCenter ) );

        //// translate to the Cartesian coordinates of our center point
        //// >>in the TiledProjection space<<
        //corners = corners.ApplyTransform(
        //    Matrix4x4.CreateTranslation( new Vector3( cartesianCenter.X, cartesianCenter.Y, 0F ) ) );

        // find the range of tiles covering the mapped rectangle
        var minTileX = CartesianToTile( corners.Min( x => x.X ) );
        var maxTileX = CartesianToTile( corners.Max( x => x.X ) );
        var minTileY = CartesianToTile( corners.Min( y => y.Y ) );
        var maxTileY = CartesianToTile( corners.Max( y => y.Y ) );

        var retVal = new MapTileList();

        for( var xTile = minTileX; xTile <= maxTileX; xTile++ )
        {
            for( var yTile = minTileY; yTile <= maxTileY; yTile++ )
            {
                var mapTile = await MapTile.CreateAsync( Projection, xTile, yTile, cancellationToken );

                if( !deferImageLoad )
                    await mapTile.GetImageAsync( cancellationToken );

                if( !retVal.Add( mapTile ) )
                    _logger.Error( "Problem adding MapTile to collection (probably differing ITiledMapScope)" );
            }
        }

        return retVal;
    }

    private int CartesianToTile( float value )
    {
        return Convert.ToInt32(Math.Floor(value / Projection.TileHeightWidth));
    }

    private async Task<MapTile> CreateMapTile( int x, int y, CancellationToken cancellationToken )
    {
        var retVal = new Cartesian(Scope);
        retVal.SetCartesian( x, y );

        return await MapTile.CreateAsync( Projection, retVal, cancellationToken );
    }
}
