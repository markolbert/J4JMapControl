using System.Numerics;
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;

namespace J4JMapLibrary.Viewport;

public class ViewportRectangle
{
    private static readonly MinMax<float> NonNegativeRange = new( 0F, float.MaxValue );

    private readonly IJ4JLogger _logger;

    private List<MapTile>? _tiles;
    private ITiledProjection? _projection;
    private float _height;
    private float _width;
    private float _centerLat;
    private float _centerLong;
    private float _heading;
    private bool _deferImageLoad;
    private bool _updateNeeded = true;

    public ViewportRectangle(
        IJ4JLogger logger
    )
    {
        Projection.ScaleChanged += Projection_ScaleChanged;

        _logger = logger;
        _logger.SetLoggedType( GetType() );
    }

    private void Projection_ScaleChanged( object? sender, int e )
    {
        _updateNeeded = true;
    }

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
            Scope = TiledMapScope.Copy( (TiledMapScope) _projection.GetScope() );
            
            _updateNeeded = true;
        }
    }

    public TiledMapScope Scope { get; private set; } = new();

    public float CenterLatitude
    {
        get => _centerLat;

        internal set
        {
            _centerLat = Scope.LatitudeRange.ConformValueToRange( value, "Latitude" );
            _updateNeeded = true;
        }
    }

    public float CenterLongitude
    {
        get => _centerLong;

        internal set
        {
            _centerLong = Scope.LongitudeRange.ConformValueToRange(value, "Longitude");
            _updateNeeded = true;
        }
    }

    public float Height
    {
        get => _height;

        internal set
        {
            _height = NonNegativeRange.ConformValueToRange(value, "Height");
            _updateNeeded = true;
        }
    }

    public float Width
    {
        get => _width;

        internal set
        {
            _width = NonNegativeRange.ConformValueToRange(value, "Width");
            _updateNeeded = true;
        }
    }

    // in degrees; north is 0/360; stored as mod 360
    public float Heading
    {
        get => _heading;

        internal set
        {
            _heading = value % 360;
            _updateNeeded = true;
        }
    }

    public async Task<List<MapTile>?> GetTilesAsync(
        CancellationToken cancellationToken,
        bool deferImageLoad = false,
        bool forceUpdate = false
    )
    {
        _deferImageLoad = deferImageLoad;

        if( _updateNeeded || forceUpdate )
            _tiles = await CreateTileCollection( cancellationToken );

        return _tiles;
    }

    private async Task<List<MapTile>?> CreateTileCollection( CancellationToken cancellationToken )
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

        var corners = new[]
        {
            new Vector3( 0, 0, 0 ),
            new Vector3( Width, 0, 0 ),
            new Vector3( Width, Height, 0 ),
            new Vector3( 0, Height, 0 ),
        };

        var vpCenter = new Vector3( Width / 2F, Height / 2F, 0 );

        // apply rotation if one is defined
        // heading == 270 is rotation == 90, hence the angle adjustment
        if( _heading != 0 )
            corners = corners.ApplyTransform(
                Matrix4x4.CreateRotationZ( ( 360 - _heading ) * MapConstants.RadiansPerDegree, vpCenter ) );

        // translate to the Cartesian coordinates of our center point
        // >>in the TiledProjection space<<
        var cartesianCenter = new Cartesian( Scope );
        cartesianCenter.SetCartesian( Scope.LatLongToCartesian( CenterLatitude, CenterLongitude ) );

        corners = corners.ApplyTransform(
            Matrix4x4.CreateTranslation( new Vector3( cartesianCenter.X, cartesianCenter.Y, 0F ) ) );

        // find the range of tiles covering the mapped rectangle
        var minCartesianX = RoundFloatToInt( corners.Min( x => x.X ) );
        var maxCartesianX = RoundFloatToInt( corners.Max( x => x.X ) );
        var minCartesianY = RoundFloatToInt( corners.Min( y => y.Y ) );
        var maxCartesianY = RoundFloatToInt( corners.Max( y => y.Y ) );

        var upperLeftTile = await CreateMapTile( minCartesianX, maxCartesianY, cancellationToken );
        var lowerRightTile = await CreateMapTile( maxCartesianX, minCartesianY, cancellationToken );

        var mapTileList = new MapTileList();

        for( var xTile = upperLeftTile.X; xTile <= lowerRightTile.X; xTile++ )
        {
            for( var yTile = upperLeftTile.Y; yTile >= lowerRightTile.Y; yTile-- )
            {
                var mapTile = await MapTile.CreateAsync( Projection, xTile, yTile, cancellationToken );

                if( !_deferImageLoad )
                    await mapTile.GetImageAsync( cancellationToken );

                if( !mapTileList.Add( mapTile ) )
                    _logger.Error("Problem adding MapTile to collection (probably differing ITiledMapScope)"  );
            }
        }

        return await mapTileList.GetBoundingBoxAsync( Projection, cancellationToken );
    }

    private int RoundFloatToInt( float value ) => Convert.ToInt32( Math.Round( value, MidpointRounding.AwayFromZero ) );

    private async Task<MapTile> CreateMapTile( int x, int y, CancellationToken cancellationToken )
    {
        var retVal = new Cartesian(Scope);
        retVal.SetCartesian( x, y );

        return await MapTile.CreateAsync( Projection, retVal, cancellationToken );
    }
}
