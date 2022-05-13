namespace J4JSoftware.MapLibrary;

public record BoundingBox
{
    public const double MinDelta = 1e-9;

    public BoundingBox(
        IMapProjection mapProjection,
        LatLong centerLatLong,
        double winUiWidth,
        double winUiHeight
    )
    {
        var ulTile = mapProjection.GetTileFromLatLong( centerLatLong, -winUiWidth / 2, -winUiHeight / 2 );
        var lrTile = mapProjection.GetTileFromLatLong( centerLatLong, winUiWidth / 2, winUiHeight / 2 );

        UpperLeft = new MultiCoordinates( ulTile, mapProjection, CoordinateOrigin.UpperLeft );
        LowerRight = new MultiCoordinates( lrTile, mapProjection, CoordinateOrigin.UpperLeft );

        HorizontalTiles = LowerRight.TilePoint.X - UpperLeft.TilePoint.X + 1;
        VerticalTiles = LowerRight.TilePoint.Y - UpperLeft.TilePoint.Y + 1;

        ZoomLevel = mapProjection.ZoomLevel;

        Width = HorizontalTiles * mapProjection.TileWidthHeight;
        Height = VerticalTiles * mapProjection.TileWidthHeight;

        ViewportCenter = centerLatLong;
        ViewportCenterPoint = mapProjection.LatLongToScreen(centerLatLong, CoordinateOrigin.UpperLeft);

        BoundingBoxCenterPoint = new DoublePoint(
            UpperLeft.ScreenPoint.GetX( CoordinateOrigin.UpperLeft )
          + HorizontalTiles * mapProjection.TileWidthHeight / 2.0,
            UpperLeft.ScreenPoint.GetY( CoordinateOrigin.UpperLeft )
          + VerticalTiles * mapProjection.TileWidthHeight / 2.0,
            CoordinateOrigin.UpperLeft,
            mapProjection );

        BoundingBoxCenter = new LatLong( mapProjection.MapRetrieverInfo )
        {
            Latitude = mapProjection.ScreenToLatitude( BoundingBoxCenterPoint ),
            Longitude = mapProjection.ScreenToLongitude( BoundingBoxCenterPoint ),
        };
    }

    public MultiCoordinates UpperLeft { get; }
    public MultiCoordinates LowerRight { get; }

    public LatLong ViewportCenter { get; }
    public LatLong BoundingBoxCenter { get; }

    // these points are in projection-space
    public DoublePoint ViewportCenterPoint { get; }
    public DoublePoint BoundingBoxCenterPoint { get; }

    public double GetCenterOffset( CoordinateAxis axis )
    {
        var viewPort = axis switch
        {
            CoordinateAxis.XAxis => ViewportCenterPoint.GetX( CoordinateOrigin.UpperLeft ),
            CoordinateAxis.YAxis => ViewportCenterPoint.GetY( CoordinateOrigin.UpperLeft ),
            _ => throw new InvalidOperationException( $"Unsupported {typeof( CoordinateAxis )} value '{axis}'" )
        };

        var boundingBox = axis switch
        {
            CoordinateAxis.XAxis => BoundingBoxCenterPoint.GetX(CoordinateOrigin.UpperLeft),
            CoordinateAxis.YAxis => BoundingBoxCenterPoint.GetY(CoordinateOrigin.UpperLeft),
            _ => throw new InvalidOperationException($"Unsupported {typeof(CoordinateAxis)} value '{axis}'")
        };

        return boundingBox - viewPort;
    }

    public int HorizontalTiles { get; }
    public int VerticalTiles { get; }
    public int ZoomLevel { get; }

    public double Width { get; }
    public double Height { get; }

    public IEnumerable<MultiCoordinates> GetTileCoordinates( IMapProjection mapProjection )
    {
        for( var yTile = UpperLeft.TilePoint.Y; yTile <= LowerRight.TilePoint.Y; yTile++ )
        {
            for( var xTile = UpperLeft.TilePoint.X; xTile <= LowerRight.TilePoint.X; xTile++ )
            {
                yield return mapProjection.GetTileCoordinates( xTile, yTile, CoordinateOrigin.UpperLeft );
            }
        }
    }
}