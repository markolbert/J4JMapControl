using System;
using Windows.Foundation;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
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
        double rotation
    )
    {
        var projectionCenter = this.LatLongToCartesian( center );

        _affineMatrix.SetRotation(rotation);

        _projSpaceTransform.Rotation = rotation;

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

    public double LatitudeToCartesian( double angle ) =>
        MercatorTransforms.LatitudeToCartesian( angle, ProjectionWidthHeight );

    public double LongitudeToCartesian( double longitude ) =>
        MercatorTransforms.LongitudeToCartesian(longitude, ProjectionWidthHeight);

    public double[] LatLongToCartesian( LatLong latLong ) =>
        new[]
        {
            LongitudeToCartesian( latLong.Longitude ),
            LatitudeToCartesian( latLong.Latitude )
        };

    public double CartesianToLatitude( double y ) =>
        MercatorTransforms.CartesianToLatitude(y, ProjectionWidthHeight );

    public double CartesianToLongitude( double x ) =>
        MercatorTransforms.CartesianToLongitude(x, ProjectionWidthHeight );

    public LatLong CartesianToLatLong(
        double x,
        double y
    ) =>
        new() { Latitude = CartesianToLatitude( y ), Longitude = CartesianToLongitude( x ) };

    public Point LatLongToScreen(LatLong latLong)
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

        return new Point( screenX, screenY );
    }

    public LatLong ScreenToLatLong( Point screenPoint )
    {
        var longitude = screenPoint.X < 0
            ? -MapRetrieverInfo.MaximumLongitude
            : screenPoint.X <= ProjectionWidthHeight
                ? 360 * (screenPoint.X / ProjectionWidthHeight - 0.5 )
                : MapRetrieverInfo.MaximumLongitude;

        var latitude = screenPoint.Y > HalfProjectionHeight
            ? MapRetrieverInfo.MaximumLatitude
            : screenPoint.Y > -HalfProjectionHeight + 1
                ? ( 2 * ( Math.Atan( Math.Exp(screenPoint.Y / MapRadius ) ) - QuarterPi ) ).RadiansToDegrees()
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
        var originPt = LatLongToScreen( origin );

        originPt.X += xOffset;
        originPt.Y += yOffset;

        return ScreenToLatLong( originPt );
    }

    public Point ToUpperLeftOrigin( Point point )
    {
        point.Y = HalfProjectionHeight - point.Y;

        return point;
    }

    public TilePoint GetTileFromScreenPoint( Point screenPoint )
    {
        return new( ScreenToTile( screenPoint.X ), ScreenToTile( screenPoint.Y ), ZoomLevel );
    }

    public TilePoint GetTileFromLatLong(
        LatLong latLong,
        double offsetX = 0,
        double offsetY = 0
    )
    {
        var screenPt = LatLongToScreen( latLong );
        screenPt = ToUpperLeftOrigin( screenPt );

        screenPt.X += offsetX;
        screenPt.Y += offsetY;

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

        var tilePoint = new TilePoint( xTile, yTile, ZoomLevel );
        var screenPoint = ToScreenPoint( tilePoint );

        return new MultiCoordinates( ScreenToLatLong( screenPoint ),
                                     tilePoint,
                                     screenPoint );
    }

    // the returned values are relative to an upper left corner origin
    // because tiles are accounted for from the upper left corner
    public Point ToScreenPoint( TilePoint tilePoint ) =>
        new Point( tilePoint.X * TileWidthHeight, tilePoint.Y * TileWidthHeight );

}
