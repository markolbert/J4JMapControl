using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

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
        var logger = J4JDeusEx.ServiceProvider.GetRequiredService<IJ4JLogger>();

        ViewportCenter = centerLatLong;
        ViewportCenterPoint = mapProjection.LatLongToScreen( centerLatLong, CoordinateOrigin.UpperLeft );

        var ulTile = mapProjection.GetTileFromLatLong( centerLatLong, -winUiWidth / 2, -winUiHeight / 2 );
        var lrTile = mapProjection.GetTileFromLatLong( centerLatLong, winUiWidth / 2, winUiHeight / 2 );

        UpperLeft = new MultiCoordinates( ulTile, mapProjection, CoordinateOrigin.UpperLeft );
        LowerRight = new MultiCoordinates( lrTile, mapProjection, CoordinateOrigin.UpperLeft );

        logger.Warning( "Creating BoundingBox, TileWidthHeight is {0}", mapProjection.TileWidthHeight );

        HorizontalTiles = LowerRight.TilePoint.X - UpperLeft.TilePoint.X + 1;
        logger.Warning( "Horizontal tiles: {0} -> {1} ({2})",
                        UpperLeft.TilePoint.X,
                        LowerRight.TilePoint.X,
                        HorizontalTiles );

        VerticalTiles = LowerRight.TilePoint.Y - UpperLeft.TilePoint.Y + 1;
        logger.Warning("Vertical tiles: {0} -> {1} ({2})",
                       UpperLeft.TilePoint.Y,
                       LowerRight.TilePoint.Y,
                       VerticalTiles);

        ZoomLevel = mapProjection.ZoomLevel;

        Width = HorizontalTiles * mapProjection.TileWidthHeight;
        Height = VerticalTiles * mapProjection.TileWidthHeight;

        BoundingBoxCenter = new LatLong( mapProjection.MapRetrieverInfo )
        {
            Latitude = ( UpperLeft.LatLong.Latitude + LowerRight.LatLong.Latitude ) / 2,
            Longitude = ( UpperLeft.LatLong.Longitude + LowerRight.LatLong.Longitude ) / 2
        };

        BoundingBoxCenterPoint = mapProjection.LatLongToScreen( BoundingBoxCenter, CoordinateOrigin.UpperLeft );
    }

    public MultiCoordinates UpperLeft { get; }
    public MultiCoordinates LowerRight { get; }

    public LatLong ViewportCenter { get; }
    public LatLong BoundingBoxCenter { get; }

    // center points are in mercator-space
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

        return viewPort - boundingBox;
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