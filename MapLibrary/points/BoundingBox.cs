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

        DesiredCenter = centerLatLong;
        DesiredCenterPoint = mapProjection.LatLongToScreen(centerLatLong, CoordinateOrigin.UpperLeft);

        CenterPoint = new DoublePoint(
            UpperLeft.ScreenPoint.GetX( CoordinateOrigin.UpperLeft )
          + HorizontalTiles * mapProjection.TileWidthHeight / 2.0,
            UpperLeft.ScreenPoint.GetY( CoordinateOrigin.UpperLeft )
          + VerticalTiles * mapProjection.TileWidthHeight / 2.0,
            CoordinateOrigin.UpperLeft,
            mapProjection );

        BoundingBoxCenter = new LatLong( mapProjection.MapRetrieverInfo )
        {
            Latitude = mapProjection.ScreenToLatitude( CenterPoint ),
            Longitude = mapProjection.ScreenToLongitude( CenterPoint ),
        };
    }

    public MultiCoordinates UpperLeft { get; }
    public MultiCoordinates LowerRight { get; }

    public LatLong DesiredCenter { get; }
    public LatLong BoundingBoxCenter { get; }

    // these points are in projection-space
    public DoublePoint DesiredCenterPoint { get; }
    public DoublePoint CenterPoint { get; }

    public double GetDesiredCenterOffset( CoordinateAxis axis )
    {
        var viewPort = axis switch
        {
            CoordinateAxis.XAxis => DesiredCenterPoint.GetX( CoordinateOrigin.UpperLeft ),
            CoordinateAxis.YAxis => DesiredCenterPoint.GetY( CoordinateOrigin.UpperLeft ),
            _ => throw new InvalidOperationException( $"Unsupported {typeof( CoordinateAxis )} value '{axis}'" )
        };

        var boundingBox = axis switch
        {
            CoordinateAxis.XAxis => CenterPoint.GetX(CoordinateOrigin.UpperLeft),
            CoordinateAxis.YAxis => CenterPoint.GetY(CoordinateOrigin.UpperLeft),
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