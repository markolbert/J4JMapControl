using System;
using Windows.Foundation;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media;

namespace J4JSoftware.MapLibrary;

public class MercatorProjection : IMapProjection
{
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

    public Point MapTileToCartesian(MapTile mapTile) =>
        new(mapTile.X * TileWidthHeight, mapTile.Y * TileWidthHeight);

    public Point MapTileCenterToCartesian( MapTile mapTile ) =>
        new( mapTile.X * TileWidthHeight + TileWidthHeight / 2, mapTile.Y * TileWidthHeight + TileWidthHeight / 2 );
}
