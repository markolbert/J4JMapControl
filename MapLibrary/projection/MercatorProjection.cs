using System;
using Windows.Foundation;
using ABI.Microsoft.UI.Composition.Interactions;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media;

namespace J4JSoftware.MapLibrary;

public class MercatorProjection : IMapProjection
{
    //private const double TwoPi = 2 * Math.PI;
    //private const double QuarterPi = Math.PI / 4;

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
            if (_mapRetrieverInfo != null)
                return _mapRetrieverInfo;

            var msg = $"Attempting to access {nameof(MapRetrieverInfo)} when it has not been initialized";

            _logger?.Fatal(msg);
            throw new NullReferenceException(msg);
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

    //public int MaximumZoom => MapRetrieverInfo.MaximumZoom;
    //public int MinimumZoom => MapRetrieverInfo.MinimumZoom;

    public int ZoomLevel
    {
        get => _zoomLevel;

        set
        {
            if (value < MapRetrieverInfo.MinimumZoom)
            {
                _logger?.Warning("Zoom level ({0}) < minimum ({1}), adjusted", value, MapRetrieverInfo.MinimumZoom);
                value = MapRetrieverInfo.MinimumZoom;
            }

            if (value > MapRetrieverInfo.MaximumZoom)
            {
                _logger?.Warning("Zoom level ({0}) > maximum ({1}), adjusted", value, MapRetrieverInfo.MaximumZoom);
                value = MapRetrieverInfo.MaximumZoom;
            }

            var changed = value != _zoomLevel;

            _zoomLevel = value;

            if (changed)
                OnZoomChanged();
        }
    }

    private void OnZoomChanged()
    {
        ZoomFactor = 2.Pow( ZoomLevel - MapRetrieverInfo.MinimumZoom );
        ProjectionWidthHeight = TileWidthHeight * ZoomFactor;
    }

    public int ZoomFactor { get; private set; }

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

    //public double MapRadius => ProjectionWidthHeight / TwoPi;

    public int ProjectionWidthHeight { get; private set; }
    private double HalfProjectionHeight => ProjectionWidthHeight / 2.0;
    public int TileWidthHeight { get; private set; }

    public Rect GetProjectionRegion(BoundingBox boundingBox)
    {
        var projectionCenter = LatLongToCartesian(boundingBox.ViewportCenter);

        var transform = new CompositeTransform
        {
            Rotation = boundingBox.Rotation,
            TranslateX = projectionCenter.X,
            TranslateY = projectionCenter.Y
        };

        var viewPortRect = new Rect(-boundingBox.Viewport.Width / 2,
                                    -boundingBox.Viewport.Height / 2,
                                    boundingBox.Viewport.Width,
                                    boundingBox.Viewport.Height);

        return transform.TransformBounds(viewPortRect);
    }

    public TileRegion GetTileRegion( BoundingBox boundingBox )
    {
        //var projectionCenter = LatLongToCartesian( center );

        //var transform = new CompositeTransform
        //{
        //    Rotation = rotation, TranslateX = projectionCenter.X, TranslateY = projectionCenter.Y
        //};

        //var viewPortRect = new Rect( -boundingBoxWidth / 2,
        //                             -boundingBoxHeight / 2,
        //                             boundingBoxWidth,
        //                             boundingBoxHeight );
        //var projectedRect = transform.TransformBounds( viewPortRect );

        var projectedRect = GetProjectionRegion( boundingBox );

        return new TileRegion( new MapTile( ScreenToTile( projectedRect.Left ),
                                            ScreenToTile( HalfProjectionHeight - projectedRect.Bottom ),
                                            ZoomLevel ),
                               new MapTile( ScreenToTile( projectedRect.Right ),
                                            ScreenToTile( HalfProjectionHeight - projectedRect.Top ),
                                            ZoomLevel ) );
    }

    private int ScreenToTile(double value)
    {
        var retVal = value < 0
            ? 0
            : value > ProjectionWidthHeight - 1
                ? ProjectionWidthHeight - 1
                : value;

        return Convert.ToInt32(Math.Floor(retVal / TileWidthHeight));
    }

    public BoundingBox GetBoundingBox(LatLong center, double viewPortWidth, double viewPortHeight, double rotation) =>
        new(this,
            new Rect(0, 0, viewPortWidth, viewPortHeight),
            center,
            rotation);

    public Point LatLongToCartesian( LatLong latLong ) =>
        new( MercatorTransforms.LongitudeToCartesian( latLong.Longitude, ProjectionWidthHeight ),
             MercatorTransforms.LatitudeToCartesian( latLong.Latitude, ProjectionWidthHeight ) );

    public LatLong CartesianToLatLong( Point screenPoint ) =>
        new()
        {
            Latitude = MercatorTransforms.CartesianToLatitude( screenPoint.Y, ProjectionWidthHeight ),
            Longitude = MercatorTransforms.CartesianToLongitude( screenPoint.X, ProjectionWidthHeight )
        };

    //public Point LatLongToScreen(LatLong latLong)
    //{
    //    // screenX and screenY use CoordinateOrigin.MiddleLeft
    //    var screenX = ProjectionWidthHeight * (0.5 + latLong.Longitude / (2 * MapRetrieverInfo.MaximumLongitude));

    //    screenX = screenX < 0
    //        ? 0.0
    //        : screenX > ProjectionWidthHeight - 1
    //            ? ProjectionWidthHeight - 1
    //            : screenX;

    //    double screenY;

    //    if (latLong.Latitude > MapRetrieverInfo.MaximumLatitude)
    //        screenY = HalfProjectionHeight;
    //    else
    //    {
    //        if (latLong.Latitude < -MapRetrieverInfo.MaximumLatitude)
    //            screenY = -HalfProjectionHeight + 1;
    //        else
    //        {
    //            var halfAngle = (latLong.Latitude / 2).DegreesToRadians();
    //            var tangent = Math.Tan(QuarterPi + halfAngle);

    //            screenY = MapRadius * Math.Log(tangent);
    //        }
    //    }

    //    return new Point(screenX, screenY);
    //}

    //public LatLong ScreenToLatLong(Point screenPoint)
    //{
    //    var longitude = screenPoint.X < 0
    //        ? -MapRetrieverInfo.MaximumLongitude
    //        : screenPoint.X <= ProjectionWidthHeight
    //            ? 360 * (screenPoint.X / ProjectionWidthHeight - 0.5)
    //            : MapRetrieverInfo.MaximumLongitude;

    //    var latitude = screenPoint.Y > HalfProjectionHeight
    //        ? MapRetrieverInfo.MaximumLatitude
    //        : screenPoint.Y > -HalfProjectionHeight + 1
    //            ? (2 * (Math.Atan(Math.Exp(screenPoint.Y / MapRadius)) - QuarterPi)).RadiansToDegrees()
    //            : -MapRetrieverInfo.MaximumLatitude;


    //    var retVal = new LatLong
    //    {
    //        Latitude = latitude,
    //        Longitude = longitude
    //    };

    //    return retVal.Capped(MapRetrieverInfo);
    //}

    public LatLong Offset(LatLong origin, double xOffset, double yOffset)
    {
        var originPt = LatLongToCartesian(origin);

        originPt.X += xOffset;
        originPt.Y += yOffset;

        return CartesianToLatLong(originPt);
    }

    public Point ToUpperLeftOrigin(Point point)
    {
        point.Y = HalfProjectionHeight - point.Y;

        return point;
    }

    //public MapTile GetTileFromScreenPoint(Point screenPoint)
    //{
    //    return new(ScreenToTile(screenPoint.X), ScreenToTile(screenPoint.Y), ZoomLevel);
    //}

    //public MapTile GetTileFromLatLong(
    //    LatLong latLong,
    //    double offsetX = 0,
    //    double offsetY = 0
    //)
    //{
    //    var screenPt = LatLongToCartesian(latLong);
    //    screenPt = ToUpperLeftOrigin(screenPt);

    //    screenPt.X += offsetX;
    //    screenPt.Y += offsetY;

    //    return GetTileFromScreenPoint(screenPt);
    //}

    //public MapTile GetTileCoordinates(int xTile, int yTile)
    //{
    //    if (xTile < 0)
    //        xTile = 0;

    //    if (xTile > ZoomFactor - 1)
    //        xTile = ZoomFactor - 1;

    //    if (yTile < 0)
    //        yTile = 0;

    //    if (yTile > ZoomFactor - 1)
    //        yTile = ZoomFactor - 1;

    //    return new MapTile(xTile, yTile, ZoomLevel);
    //}

    // the returned values are relative to an upper left corner origin
    // because tiles are accounted for from the upper left corner
    public Point MapTileToCartesian(MapTile mapTile) =>
        new Point(mapTile.X * TileWidthHeight, mapTile.Y * TileWidthHeight);
}
