using System;
using System.ComponentModel;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media;

namespace J4JSoftware.MapLibrary;

public class MercatorProjection : IMapProjection
{
    private const double TwoPi = 2 * Math.PI;
    private const double QuarterPi = Math.PI / 4;

    private readonly AffineMatrix _affineMatrix;
    private readonly CompositeTransform _projSpaceTransform = new();
    private readonly IJ4JLogger? _logger;

    private MapRetrieverInfo? _mapRetrieverInfo;
    private double _viewPortWidth;
    private int _zoomLevel;

    public MercatorProjection()
    {
        _affineMatrix = J4JDeusEx.ServiceProvider.GetRequiredService<AffineMatrix>();

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

    public double[] ToProjectionSpace( double[] controlVector ) => _affineMatrix.Matrix.DotProduct( controlVector );

    public TileRegion GetTileRegion(
        LatLong center,
        double boundingBoxWidth,
        double boundingBoxHeight,
        double rotation,
        AngleMeasure angleMeasure = AngleMeasure.Degrees
    )
    {
        var projectionCenter = this.LatLongToCartesian( center, angleMeasure );

        _affineMatrix.SetRotation(rotation, angleMeasure);

        _projSpaceTransform.Rotation = angleMeasure switch
        {
            AngleMeasure.Degrees => rotation,
            AngleMeasure.Radians => rotation.RadiansToDegrees(),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( AngleMeasure )} value '{angleMeasure}'" )
        };

        var left = int.MaxValue;
        var top = int.MaxValue;
        var right = int.MinValue;
        var bottom = int.MinValue;

        // upper left corner of bounding box
        var tileCoords = GetTileCoordinates(-boundingBoxWidth / 2, boundingBoxHeight / 2, projectionCenter);
        MinMax();

        // upper right corner of bounding box
        tileCoords = GetTileCoordinates(boundingBoxWidth / 2, boundingBoxHeight / 2, projectionCenter);
        MinMax();
        
        // lower left corner of bounding box
        tileCoords = GetTileCoordinates(-boundingBoxWidth / 2, -boundingBoxHeight / 2, projectionCenter);
        MinMax();

        // lower right corner of bounding box
        tileCoords = GetTileCoordinates(boundingBoxWidth / 2, -boundingBoxHeight / 2, projectionCenter);
        MinMax();

        return new TileRegion( new TilePoint( left, top, ZoomLevel ),
                               new TilePoint( right, bottom, ZoomLevel ) );

        void MinMax()
        {
            if (tileCoords.X < left)
                left = tileCoords.X;
            if (tileCoords.Y < top)
                top = tileCoords.Y;
            if (tileCoords.X > right)
                right = tileCoords.X;
            if (tileCoords.Y > bottom)
                bottom = tileCoords.Y;
        }
    }

    private (int X, int Y) GetTileCoordinates( double xCenterOffset, double yCenterOffset, double[] projectionCenter )
    {
        projectionCenter = projectionCenter.ExpandToAffine();

        var cornerVector = AffineExtensions.CreateAffineVector( xCenterOffset, yCenterOffset );
        var offsetProjection = ToProjectionSpace( cornerVector ).ExpandToAffine();
        var cornerProjection = offsetProjection.Add( projectionCenter ).ExpandToAffine();

        // in determining tile coordinates, remember to account for the fact that our Cartesian 
        // coordinates have an origin at the middle left of projection space, while tile coordinates
        // have an origin at the upper left of their space
        return ( ScreenToTile( cornerProjection[ 0 ] ), ScreenToTile( HalfProjectionHeight - cornerProjection[ 1 ] ) );
    }

    private int ScreenToTile( double value )
    {
        var retVal = value < 0
            ? 0
            : value > ProjectionWidthHeight - 1
                ? ProjectionWidthHeight - 1
                : value;

        return Convert.ToInt32( Math.Floor( retVal / TileWidthHeight ) );
    }

    public double LatitudeToCartesian( double angle, AngleMeasure angleMeasure = AngleMeasure.Degrees ) =>
        MercatorTransforms.LatitudeToCartesian( angle, ProjectionWidthHeight, angleMeasure );

    public double LongitudeToCartesian( double angle, AngleMeasure angleMeasure = AngleMeasure.Degrees ) =>
        MercatorTransforms.LongitudeToCartesian(angle, ProjectionWidthHeight, angleMeasure);

    public double[] LatLongToCartesian( LatLong latLong, AngleMeasure angleMeasure = AngleMeasure.Degrees ) =>
        new[]
        {
            LongitudeToCartesian( latLong.Longitude, angleMeasure ),
            LatitudeToCartesian( latLong.Latitude, angleMeasure )
        };

    public double CartesianToLatitude( double y, AngleMeasure angleMeasure = AngleMeasure.Degrees ) =>
        MercatorTransforms.CartesianToLatitude(y, ProjectionWidthHeight, angleMeasure);

    public double CartesianToLongitude( double x, AngleMeasure angleMeasure = AngleMeasure.Degrees ) =>
        MercatorTransforms.CartesianToLongitude(x, ProjectionWidthHeight, angleMeasure);

    public LatLong CartesianToLatLong(
        double x,
        double y,
        AngleMeasure angleMeasure = AngleMeasure.Degrees
    ) =>
        new LatLong
        {
            Latitude = CartesianToLatitude(y, angleMeasure),
            Longitude = CartesianToLongitude(x, angleMeasure)
        };

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


        var retVal = new LatLong
        {
            Latitude = latitude,
            Longitude = longitude
        };

        return retVal.Capped( MapRetrieverInfo );
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

        return new( ScreenToTile( screenX ), ScreenToTile( screenY ), ZoomLevel );
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

        return new MultiCoordinates( new TilePoint( xTile, yTile, ZoomLevel),
                                     this,
                                     origin );
    }

}
