using System.Collections.ObjectModel;
using System.Numerics;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class ViewportRectangle
{
#pragma warning disable CA2211
    public static MinMax<int> XRange = new( 0, int.MaxValue );
    public static MinMax<int> YRange = new(0, int.MaxValue);
#pragma warning restore CA2211

    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly List<MapTile> _tiles = new();
    private readonly IJ4JLogger _logger;

    private float _heading;
    private bool _deferImageLoad;

    public ViewportRectangle(
        ITiledProjection projection,
        int height,
        int width,
        int maxRequestLatency,
        IJ4JLogger logger
    )
    {
        Projection = projection;
        SetHeightWidth( height, width );

        _cancellationTokenSource = new();
        _cancellationTokenSource.CancelAfter( maxRequestLatency <= 0 ? 0 : maxRequestLatency );

        Projection.ScaleChanged += ( _, _ ) => CreateTileCollection( _cancellationTokenSource.Token ).Wait();

        Center = new LatLong( projection.Metrics );
        Center.Changed += (_, _) => CreateTileCollection(_cancellationTokenSource.Token).Wait();

        _logger = logger;
        _logger.SetLoggedType( GetType() );
    }

    public bool UpdateNeeded { get; private set; } = true;

    public ITiledProjection Projection { get; }

    public int Height { get; private set; }
    public int Width { get; private set; }

    // the CartesianCenter and CartesianCorners use traditional mathematical nomenclature,
    // with positive display Y values being deemed negative cartesian Y values (i.e.,
    // because display Y coordinates increase moving >>down<< the display
    public Vector3 CartesianCenter { get; private set; } = Vector3.Zero;
    public Vector3[] CartesianCorners { get; private set; } = new Vector3[4];

    public LatLong Center { get; }

    // in degrees; north is 0/360
    public float Heading
    {
        get => _heading;

        set
        {
            _heading = value % 360;
            UpdateNeeded = true;
        }
    }

    public void SetHeightWidth(int? height, int? width)
    {
        if (height == null && width == null)
            return;

        // Metrics is about to change, but its XRange and YRange limits never do
        if (height.HasValue)
            Height = InternalExtensions.ConformValueToRange(height.Value, YRange, "Y");

        if (width.HasValue)
            Width = InternalExtensions.ConformValueToRange(width.Value, XRange, "X");

        CartesianCenter = new Vector3( Width / 2F, -Height / 2F, 0 );
        CartesianCorners = new Vector3[]
        {
            new( 0, 0, 0 ), new( Width, 0, 0 ), new( Width, -Height, 0 ), new( 0, -Height, 0 ),
        };

        UpdateNeeded = true;
    }

    public async Task<ReadOnlyCollection<MapTile>> GetTilesAsync(
        CancellationToken cancellationToken,
        bool deferImageLoad = false,
        bool forceUpdate = false
    )
    {
        _deferImageLoad = deferImageLoad;

        if( UpdateNeeded || forceUpdate )
            await CreateTileCollection( cancellationToken );

        return _tiles.AsReadOnly();
    }

    private async Task CreateTileCollection( CancellationToken cancellationToken )
    {
        _tiles.Clear();

        if( Height == 0 || Width == 0 )
            return;

        var projCorners = new Vector3[ 4 ];
        CartesianCorners.CopyTo( projCorners, 0 );

        // apply rotation if one is defined
        // heading == 270 is rotation == 90, hence the angle adjustment
        if( _heading != 0 )
            projCorners = projCorners.ApplyTransform(
                Matrix4x4.CreateRotationZ( ( 360 - _heading ) * MapConstants.RadiansPerDegree, CartesianCenter ) );

        // translate to the Cartesian coordinates of our center point
        // >>in the TiledProjection space<<
        var cartesianCenter = new Cartesian( Projection.Metrics );
        cartesianCenter.SetCartesian( Projection.Metrics.LatLongToCartesian( Center ) );

        projCorners = projCorners.ApplyTransform(
            Matrix4x4.CreateTranslation( new Vector3( cartesianCenter.X, cartesianCenter.Y, 0F ) ) );

        // find the range of tiles covering the mapped rectangle
        var minCartesianX = RoundFloatToInt( projCorners.Min( x => x.X ) );
        var maxCartesianX = RoundFloatToInt( projCorners.Max( x => x.X ) );
        var minCartesianY = RoundFloatToInt( projCorners.Min( y => y.Y ) );
        var maxCartesianY = RoundFloatToInt( projCorners.Max( y => y.Y ) );

        var upperLeftTile = await CreateMapTile( minCartesianX, maxCartesianY, cancellationToken );
        var lowerRightTile = await CreateMapTile( maxCartesianX, minCartesianY, cancellationToken );

        for( var xTile = upperLeftTile.X; xTile <= lowerRightTile.X; xTile++ )
        {
            for( var yTile = upperLeftTile.Y; yTile >= lowerRightTile.Y; yTile-- )
            {
                var mapTile = await MapTile.CreateAsync( Projection, xTile, yTile, cancellationToken );

                if( !_deferImageLoad )
                    await mapTile.GetImageAsync( cancellationToken );

                _tiles.Add( mapTile );
            }
        }
    }

    private int RoundFloatToInt( float value ) => Convert.ToInt32( Math.Round( value, MidpointRounding.AwayFromZero ) );

    private async Task<MapTile> CreateMapTile( int x, int y, CancellationToken cancellationToken )
    {
        var retVal = new Cartesian(Projection.Metrics);
        retVal.SetCartesian( x, y );

        return await MapTile.CreateAsync( Projection, retVal, cancellationToken );
    }
}
