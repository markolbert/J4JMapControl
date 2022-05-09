using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.MapLibrary;

public class MercatorProjection : IMapProjection
{
    private const double TwoPi = 2 * Math.PI;
    private const double QuarterPi = Math.PI / 4;

    public event EventHandler<int>? ZoomChanged;

    private readonly IJ4JLogger? _logger;

    private MapRetrieverInfo? _mapRetrieverInfo;
    private double _mapWidth;
    private double _mapHeight;
    private double _mapRadius;
    private int _zoomLevel;

    public MercatorProjection()
    {
        _logger = J4JDeusEx.ServiceProvider.GetRequiredService<IJ4JLogger>();
        _logger?.SetLoggedType(GetType());
    }

    private MapRetrieverInfo GetMapRetrieverInfo( IMapContext mapContext )
    {
        if( mapContext.MapRetriever != null )
            return mapContext.MapRetriever.MapRetrieverInfo;

        var msg =
            $"Attempting to create instance of {nameof( MercatorProjection )} from an undefined {nameof( IMapContext )}";

        _logger?.Fatal(msg);

        throw new ArgumentException( msg );
    }

    public MapRetrieverInfo MapRetrieverInfo
    {
        get
        {
            if( _mapRetrieverInfo != null )
                return _mapRetrieverInfo;

            var msg = $"Attempting to access {nameof(MapRetrieverInfo)} when it has not been initialized";

            _logger?.Fatal( msg );
            throw new NullReferenceException( msg );
        }

        set
        {
            var curZoom = _zoomLevel;

            _mapRetrieverInfo = value;
            OnMapRetrieverInfoChanged();

            // validate zoom level for new retriever info
            ZoomLevel = curZoom;
        }
    }

    private void OnMapRetrieverInfoChanged()
    {
        TileWidthHeight = MapRetrieverInfo.DefaultBitmapWidthHeight;
        NumTiles = 2.Pow(ZoomLevel - MinimumZoom);
        ProjectionWidthHeight = TileWidthHeight * NumTiles;
    }

    public int MaximumZoom => MapRetrieverInfo.MaximumZoom;
    public int MinimumZoom => MapRetrieverInfo.MinimumZoom;

    public int ZoomLevel
    {
        get => _zoomLevel;

        set
        {
            if (value < MinimumZoom)
            {
                _logger?.Warning("Zoom level ({0}) < minimum ({1}), adjusted", value, MinimumZoom);
                value = MinimumZoom;
            }

            if (value > MaximumZoom)
            {
                _logger?.Warning( "Zoom level ({0}) > maximum ({1}), adjusted", value, MaximumZoom );
                value = MaximumZoom;
            }

            var changed = value != _zoomLevel;

            _zoomLevel = value;

            if( changed )
                ZoomChanged?.Invoke( this, _zoomLevel );
        }
    }

    public double MapWidth
    {
        get => _mapWidth;

        set
        {
            if( value <= 0 )
            {
                _logger?.Error( "Map width ({0}) must be > 0, ignoring change", value );
                return;
            }

            _mapWidth = value;
            _mapRadius = _mapWidth / TwoPi;

            MaximumMapHeight = ScreenFromLatitude( -MapRetrieverInfo.MaximumLatitude );
        }
    }

    public double MaximumMapHeight { get; private set; }

    public double MapHeight
    {
        get => _mapHeight;

        set
        {
            if (value <= 0)
            {
                _logger?.Error("Map height ({0}) must be > 0, ignoring change", value);
                return;
            }

            if( value > MaximumMapHeight )
            {
                _logger?.Error( "Map height ({0}) exceeds maximum ({1}), ignoring change", value, MaximumMapHeight );
                return;
            }

            _mapHeight = value;
        }
    }

    public int ProjectionWidthHeight { get; private set; }
    public int TileWidthHeight { get; private set; }
    public int NumTiles { get; private set; }

    // screenX can range from 0 to MapWidth
    public double LongitudeFromScreen( double screenX )
    {
        // ensure point is within bounds
        if (screenX < 0)
        {
            _logger?.Error( "X coordinate ({0}) < 0, returning minimum longitude ({1})",
                            screenX,
                            -MapRetrieverInfo.MaximumLongitude );

            return -MapRetrieverInfo.MaximumLongitude;
        }

        if( screenX <= ProjectionWidthHeight - 1 )
            return 360 * ( screenX / ProjectionWidthHeight - 0.5 );

        _logger?.Error( "X coordinate ({0}) >= {1}, returning maximum longitude ({2})",
                        screenX,
                        ProjectionWidthHeight,
                        MapRetrieverInfo.MaximumLongitude );

        return MapRetrieverInfo.MaximumLongitude;
    }

    // longDegrees can range from -MapRetrieverInfo.MaximumLongitude to MapRetrieverInfo.MaximumLongitude
    public double ScreenFromLongitude( double longDegrees )
    {
        if( longDegrees < -MapRetrieverInfo.MaximumLongitude )
        {
            _logger?.Error( "Longitude less than minimum ({0}), returning 0.0", -MapRetrieverInfo.MaximumLongitude );
            return 0.0;
        }

        if( longDegrees <= MapRetrieverInfo.MaximumLongitude )
            return MapWidth * ( 0.5 + longDegrees / MapRetrieverInfo.MaximumLongitude );

        _logger?.Error( "Longitude exceeds maximum ({0}), returning map width ({1})",
                        MapRetrieverInfo.MaximumLongitude,
                        MapWidth );
        
        return MapWidth;
    }

    // screenY can range from 0 to MaxScreenHeight
    public double LatitudeFromScreen( double screenY )
    {
        if( screenY < 0 )
        {
            _logger?.Error( "Y coordinate ({0}) < 0, returning maximum latitude ({1})",
                            screenY,
                            MapRetrieverInfo.MaximumLatitude );

            return MapRetrieverInfo.MaximumLatitude;
        }

        if( screenY <= MaximumMapHeight )
            return ( Math.Atan( Math.Exp( screenY / _mapRadius ) ) - QuarterPi ).RadiansToDegrees();

        _logger?.Error( "Y coordinate ({0}) > maximum ({1}), returning minimum latitude ({2})",
                        screenY,
                        MaximumMapHeight,
                        -MapRetrieverInfo.MaximumLatitude );

        return -MapRetrieverInfo.MaximumLatitude;
    }

    // latDegrees can range from -MapRetrieverInfo.MaximumLatitude to MapRetrieverInfo.MaximumLatitude
    public double ScreenFromLatitude( double latDegrees )
    {
        if( latDegrees > MapRetrieverInfo.MaximumLatitude )
        {
            _logger?.Error( "Latitude ({0}) exceeds maximum ({1}), returning 0",
                            latDegrees,
                            MapRetrieverInfo.MaximumLatitude );
            return 0.0;
        }

        if( latDegrees < -MapRetrieverInfo.MaximumLatitude )
        {
            _logger?.Error( "Latitude ({0}) less than minimum ({1}), returning maximum map height ({2})",
                            latDegrees,
                            -MapRetrieverInfo.MaximumLatitude,
                            MaximumMapHeight );

            return MaximumMapHeight;
        }

        double tangent;

        try
        {
            tangent = Math.Tan( QuarterPi + ( latDegrees / 2 ).DegreesToRadians() );
        }
        catch( OverflowException )
        {
            return MaximumMapHeight;
        }

        return _mapRadius * Math.Log( tangent );
    }

    public LatLong ScreenPointToLatLong( DoublePoint point ) =>
        new( MapRetrieverInfo )
        {
            Latitude = LatitudeFromScreen( point.Y ), 
            Longitude = LongitudeFromScreen( point.X )
        };

    public DoublePoint LatLongToScreenPoint( LatLong latLong ) =>
        new( ScreenFromLongitude( latLong.Longitude ), ScreenFromLatitude( latLong.Latitude ) );

    public TilePoint GetTileFromScreenPoint( DoublePoint point )
    {
        if( point.X < 0 )
        {
            _logger?.Error( "X coordinate ({0}) < 0, setting to 0", point.X );
            point = point with { X = 0 };
        }

        if( point.Y >= 0 )
            return new( point.X / TileWidthHeight, point.Y / TileWidthHeight );

        _logger?.Error( "Y coordinate ({0}) < 0, setting to 0", point.Y );

        point = point with { Y = 0 };

        return new( point.X / TileWidthHeight, point.Y / TileWidthHeight );
    }

    public TilePoint GetTileFromLatLong( LatLong latLong )
    {
        var screenPt = new DoublePoint( ScreenFromLongitude( latLong.Longitude ),
                                        ScreenFromLatitude( latLong.Latitude ) );

        return GetTileFromScreenPoint( screenPt );
    }
}
