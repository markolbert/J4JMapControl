using System.ComponentModel;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.MapLibrary;

public class MercatorProjection : IMapProjection
{
    private const double TwoPi = 2 * Math.PI;
    private const double QuarterPi = Math.PI / 4;

    private readonly IJ4JLogger? _logger;

    private MapRetrieverInfo? _mapRetrieverInfo;
    private double _viewPortWidth;
    private int _zoomLevel;

    public MercatorProjection()
    {
        _logger = J4JDeusEx.ServiceProvider.GetRequiredService<IJ4JLogger>();
        _logger?.SetLoggedType(GetType());
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

        OnZoomChanged();
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
                OnZoomChanged();
        }
    }

    private void OnZoomChanged()
    {
        ZoomFactor = 2.Pow(ZoomLevel - MinimumZoom);

        ProjectionWidthHeight = TileWidthHeight * ZoomFactor;
    }

    public double ViewportWidth
    {
        get => _viewPortWidth;

        set
        {
            if (value <= 0)
            {
                _logger?.Error("Viewport width ({0}) must be > 0, ignoring change", value);
                return;
            }

            _viewPortWidth = value;
        }
    }

    public double MapRadius => ProjectionWidthHeight / TwoPi;

    public int ProjectionWidthHeight { get; private set; }
    private double HalfProjectionHeight => ProjectionWidthHeight / 2.0;
    public int TileWidthHeight { get; private set; }
    public int ZoomFactor { get; private set; }

    public DoublePoint LatLongToScreen(LatLong latLong, CoordinateOrigin origin)
    {
        // screenX and screenY use CoordinateOrigin.MiddleLeft
        var screenX = ProjectionWidthHeight * (0.5 + latLong.Longitude / (2 * MapRetrieverInfo.MaximumLongitude));

        screenX = screenX < 0
            ? 0.0
            : screenX > ProjectionWidthHeight - 1
                ? ProjectionWidthHeight - 1
                : screenX;

        double screenY;

        if (latLong.Latitude > MapRetrieverInfo.MaximumLatitude)
            screenY = HalfProjectionHeight;
        else
        {
            if( latLong.Latitude < -MapRetrieverInfo.MaximumLatitude )
                screenY = -HalfProjectionHeight + 1;
            else
            {
                var halfAngle = ( latLong.Latitude / 2 ).DegreesToRadians();
                var tangent = Math.Tan( QuarterPi + halfAngle );

                screenY = MapRadius * Math.Log( tangent );
            }
        }

        var retVal = new DoublePoint(origin, this);

        retVal.Set( screenX, screenY, CoordinateOrigin.MiddleLeft );

        return retVal;
    }

    public LatLong ScreenToLatLong( DoublePoint screenPoint )
    {
        // ensure point is within bounds
        var (screenX, screenY) = screenPoint.GetValues(CoordinateOrigin.MiddleLeft);

        var longitude = screenX < 0
            ? -MapRetrieverInfo.MaximumLongitude
            : screenX <= ProjectionWidthHeight
                ? 360 * ( screenX / ProjectionWidthHeight - 0.5 )
                : MapRetrieverInfo.MaximumLongitude;

        var latitude = screenY > HalfProjectionHeight
            ? MapRetrieverInfo.MaximumLatitude
            : screenY > -HalfProjectionHeight + 1
                ? ( 2 * ( Math.Atan( Math.Exp( screenY / MapRadius ) ) - QuarterPi ) ).RadiansToDegrees()
                : -MapRetrieverInfo.MaximumLatitude;


        return new LatLong( MapRetrieverInfo )
        {
            Latitude = latitude,
            Longitude = longitude
        };
    }

    public LatLong Offset( LatLong origin, double xOffset, double yOffset )
    {
        var originPt = LatLongToScreen( origin, CoordinateOrigin.UpperLeft );

        originPt.Increment( xOffset, yOffset );

        return ScreenToLatLong( originPt );
    }

    public double ChangeOrigin( double value, CoordinateAxis axis ) =>
        axis switch
        {
            CoordinateAxis.XAxis => value,
            CoordinateAxis.YAxis => HalfProjectionHeight - value,
            _ => throw new InvalidEnumArgumentException( $"Unsupported {typeof( CoordinateAxis )} value '{axis}'" )
        };

    public TilePoint GetTileFromScreenPoint( DoublePoint screenPoint )
    {
        var (screenX, screenY) = screenPoint.GetValues( CoordinateOrigin.UpperLeft );

        if( screenX < 0 )
            screenX = 0;

        if( screenX > ProjectionWidthHeight - 1)
            screenX = ProjectionWidthHeight - 1;

        if( screenY < 0 )
            screenY = 0;

        if( screenY > ProjectionWidthHeight - 1 )
            screenY = ProjectionWidthHeight - 1;

        return new( screenX / TileWidthHeight, screenY / TileWidthHeight, ZoomLevel );
    }

    public TilePoint GetTileFromLatLong(
        LatLong latLong,
        double offsetX = 0,
        double offsetY = 0
    )
    {
        var screenPt = LatLongToScreen( latLong, CoordinateOrigin.UpperLeft );
        screenPt.Increment( offsetX, offsetY );

        return GetTileFromScreenPoint( screenPt );
    }

    public MultiCoordinates GetTileCoordinates(int xTile, int yTile, CoordinateOrigin origin)
    {
        if (xTile < 0)
            xTile = 0;

        if (xTile > ZoomFactor - 1)
            xTile = ZoomFactor - 1;

        if (yTile < 0)
            yTile = 0;

        if (yTile > ZoomFactor - 1)
            yTile = ZoomFactor - 1;

        return new MultiCoordinates( new TilePoint( xTile, yTile, ZoomLevel ),
                                     this,
                                     origin );
    }

}
