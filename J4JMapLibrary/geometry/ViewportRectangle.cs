using System.Collections.ObjectModel;
using System.Numerics;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class ViewportRectangle
{
    public static MinMax<int> XRange = new MinMax<int>( 0, int.MaxValue );
    public static MinMax<int> YRange = new MinMax<int>(0, int.MaxValue);

    private readonly List<MapTile> _tiles = new();
    private readonly IJ4JLogger _logger;

    private float _rotation;

    public ViewportRectangle(
        ITiledProjection projection,
        int height,
        int width,
        IJ4JLogger logger
    )
    {
        Projection = projection;
        SetHeightWidth( height, width );
        Projection.ScaleChanged += ( _, _ ) => CreateTileCollection();

        Center = new LatLong( projection.Metrics );
        Center.Changed += (_, _) => CreateTileCollection();

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

    public float Rotation
    {
        get => _rotation;

        set
        {
            _rotation = value;
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

    public async Task<ReadOnlyCollection<MapTile>> GetTilesAsync( bool forceUpdate )
    {
        if( UpdateNeeded || forceUpdate)
            await CreateTileCollection();

        return _tiles.AsReadOnly();
    }

    private async Task CreateTileCollection()
    {
        _tiles.Clear();

        if( Height == 0 || Width == 0 )
            return;

        var projCorners = new Vector3[ 4 ];
        CartesianCorners.CopyTo( projCorners, 0 );

        // apply rotation if one is defined
        if( _rotation != 0 )
            projCorners = projCorners.ApplyTransform(
                Matrix4x4.CreateRotationZ( _rotation * MapConstants.RadiansPerDegree, CartesianCenter ) );

        // translate to the Cartesian coordinates of our center point
        // >>in the TiledProjection space<<
        var projPoint = new MapPoint( Projection.Metrics );
        projPoint.LatLong.SetLatLong( Center );

        projCorners = projCorners.ApplyTransform(
            Matrix4x4.CreateTranslation( new Vector3( projPoint.Cartesian.X, projPoint.Cartesian.Y, 0F ) ) );

        // find the range of tiles covering the mapped rectangle
        var minCartesianX = RoundFloatToInt( projCorners.Min( x => x.X ) );
        var maxCartesianX = RoundFloatToInt(projCorners.Max( x => x.X ));
        var minCartesianY = RoundFloatToInt(projCorners.Min( y => y.Y ));
        var maxCartesianY = RoundFloatToInt(projCorners.Max(y => y.Y));

        var upperLeftTile = await CreateMapTile(minCartesianX, maxCartesianY);
        var lowerRightTile = await CreateMapTile( maxCartesianX, minCartesianY );

        for( var xTile = upperLeftTile.X; xTile <= lowerRightTile.X; xTile++ )
        {
            for( var yTile = upperLeftTile.Y; yTile >= lowerRightTile.Y; yTile-- )
            {
                _tiles.Add( await MapTile.CreateAsync( Projection, xTile, yTile ) );
            }
        }
    }

    private int RoundFloatToInt( float value ) => Convert.ToInt32( Math.Round( value, MidpointRounding.AwayFromZero ) );

    private async Task<MapTile> CreateMapTile( int x, int y )
    {
        var retVal = new Cartesian(Projection.Metrics);
        retVal.SetCartesian( x, y );

        return await MapTile.CreateAsync(Projection, retVal);
    }
}
