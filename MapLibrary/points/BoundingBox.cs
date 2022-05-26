using System;
using System.Collections.Generic;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.MapLibrary;

public record BoundingBox
{
    public const double MinDelta = 1e-9;

    public BoundingBox(
        IMapProjection mapProjection,
        LatLong center,
        double controlWidth,
        double controlHeight
    )
    {
        ControlWidth = controlWidth;
        ControlHeight = controlHeight;

        var tileRegion = mapProjection.GetTileRegion( center, controlWidth, controlHeight, 0 );

        var ulTile = mapProjection.GetTileFromLatLong( center, -controlWidth / 2, -controlHeight / 2 );
        var lrTile = mapProjection.GetTileFromLatLong( center, controlWidth / 2, controlHeight / 2 );

        UpperLeft = new MultiCoordinates( ulTile, mapProjection, CoordinateOrigin.UpperLeft );
        LowerRight = new MultiCoordinates( lrTile, mapProjection, CoordinateOrigin.UpperLeft );

        HorizontalTiles = LowerRight.TilePoint.X - UpperLeft.TilePoint.X + 1;
        VerticalTiles = LowerRight.TilePoint.Y - UpperLeft.TilePoint.Y + 1;

        ZoomLevel = mapProjection.ZoomLevel;

        ProjectionWidth = HorizontalTiles * mapProjection.TileWidthHeight;
        ProjectionHeight = VerticalTiles * mapProjection.TileWidthHeight;

        DesiredCenter = center;
        DesiredCenterPoint = mapProjection.LatLongToScreen(center, CoordinateOrigin.UpperLeft);

        var (xUpperLeft, yUpperLeft) = UpperLeft.ScreenPoint.GetValues( CoordinateOrigin.UpperLeft );

        CenterPoint = new DoublePoint(
            xUpperLeft + HorizontalTiles * mapProjection.TileWidthHeight / 2.0,
            yUpperLeft + + VerticalTiles * mapProjection.TileWidthHeight / 2.0,
            CoordinateOrigin.UpperLeft,
            mapProjection);

        BoundingBoxCenter = mapProjection.ScreenToLatLong( CenterPoint );
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
        var (viewPortX, viewPortY) = DesiredCenterPoint.GetValues( CoordinateOrigin.UpperLeft );
        var (bBoxX, bBoxY) = CenterPoint.GetValues( CoordinateOrigin.UpperLeft );

        return axis switch
        {
            CoordinateAxis.XAxis => bBoxX - viewPortX,
            CoordinateAxis.YAxis => bBoxY - viewPortY,
            _ => throw new InvalidOperationException( $"Unsupported {typeof( CoordinateAxis )} value '{axis}'" )
        };
    }

    public int HorizontalTiles { get; }
    public int VerticalTiles { get; }
    public int ZoomLevel { get; }

    public double ProjectionWidth { get; }
    public double ProjectionHeight { get; }

    public double ControlWidth { get; }
    public double ControlHeight { get; }

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