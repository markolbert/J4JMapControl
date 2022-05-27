using System;
using System.Collections.Generic;
using Windows.Foundation;

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

        var ulScreenPoint = mapProjection.ToScreenPoint( ulTile );
        UpperLeft = new MultiCoordinates( mapProjection.ScreenToLatLong( ulScreenPoint ), ulTile, ulScreenPoint );

        var lrScreenPoint = mapProjection.ToScreenPoint( lrTile );
        LowerRight = new MultiCoordinates( mapProjection.ScreenToLatLong( lrScreenPoint ), lrTile, lrScreenPoint );

        HorizontalTiles = LowerRight.TilePoint.X - UpperLeft.TilePoint.X + 1;
        VerticalTiles = LowerRight.TilePoint.Y - UpperLeft.TilePoint.Y + 1;

        ZoomLevel = mapProjection.ZoomLevel;

        ProjectionWidth = HorizontalTiles * mapProjection.TileWidthHeight;
        ProjectionHeight = VerticalTiles * mapProjection.TileWidthHeight;

        DesiredCenter = center;

        // the coordinates returned by LatLongToScreen are in projection space, they
        // need to be converted to control/Windows space
        var desiredCenterPt = mapProjection.LatLongToScreen(center);
        DesiredCenterPoint = mapProjection.ToUpperLeftOrigin( desiredCenterPt );

        CenterPoint = new Point( UpperLeft.ScreenPoint.X + HorizontalTiles * mapProjection.TileWidthHeight / 2.0,
                                 UpperLeft.ScreenPoint.Y + VerticalTiles * mapProjection.TileWidthHeight / 2.0 );

        BoundingBoxCenter = mapProjection.ScreenToLatLong( CenterPoint );
    }

    public MultiCoordinates UpperLeft { get; }
    public MultiCoordinates LowerRight { get; }

    public LatLong DesiredCenter { get; }
    public LatLong BoundingBoxCenter { get; }

    // these points are in projection-space
    public Point DesiredCenterPoint { get; }
    public Point CenterPoint { get; }

    public double GetDesiredCenterOffset( CoordinateAxis axis ) =>
        axis switch
        {
            CoordinateAxis.XAxis => CenterPoint.X - DesiredCenterPoint.X,
            CoordinateAxis.YAxis => CenterPoint.Y - DesiredCenterPoint.Y,
            _ => throw new InvalidOperationException( $"Unsupported {typeof( CoordinateAxis )} value '{axis}'" )
        };

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