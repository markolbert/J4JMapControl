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
        double viewPortWidth,
        double viewPortHeight,
        double rotation
    )
    {
        Viewport = new Rect( 0, 0, viewPortWidth, viewPortHeight );

        TileRegion = mapProjection.GetTileRegion( center, viewPortWidth, viewPortHeight, rotation );   

        //var ulTile = mapProjection.GetTileFromLatLong( center, -controlWidth / 2, -controlHeight / 2 );
        //var lrTile = mapProjection.GetTileFromLatLong( center, controlWidth / 2, controlHeight / 2 );

        //var ulScreenPoint = mapProjection.ToScreenPoint( ulTile );
        //UpperLeft = new MultiCoordinates( mapProjection.ScreenToLatLong( ulScreenPoint ), ulTile, ulScreenPoint );

        //var lrScreenPoint = mapProjection.ToScreenPoint( lrTile );
        //LowerRight = new MultiCoordinates( mapProjection.ScreenToLatLong( lrScreenPoint ), lrTile, lrScreenPoint );

        //HorizontalTiles = LowerRight.TilePoint.X - UpperLeft.TilePoint.X + 1;
        //VerticalTiles = LowerRight.TilePoint.Y - UpperLeft.TilePoint.Y + 1;

        //ZoomLevel = mapProjection.ZoomLevel;

        //ProjectionWidth = HorizontalTiles * mapProjection.TileWidthHeight;
        //ProjectionHeight = VerticalTiles * mapProjection.TileWidthHeight;

        ViewportCenter = center;

        // the coordinates returned by LatLongToScreen are in projection space, they
        // need to be converted to control/Windows space
        var desiredCenterPt = mapProjection.LatLongToScreen(center);
        ViewportCenterPoint = mapProjection.ToUpperLeftOrigin( desiredCenterPt );

        //CenterPoint = new Point( UpperLeft.ScreenPoint.X + HorizontalTiles * mapProjection.TileWidthHeight / 2.0,
        //                         UpperLeft.ScreenPoint.Y + VerticalTiles * mapProjection.TileWidthHeight / 2.0 );

        //BoundingBoxCenter = mapProjection.ScreenToLatLong( CenterPoint );
    }

    public Rect Viewport { get; }
    public TileRegion TileRegion { get; }

    //public MultiCoordinates UpperLeft { get; }
    //public MultiCoordinates LowerRight { get; }

    public LatLong ViewportCenter { get; }
    //public LatLong BoundingBoxCenter { get; }

    // this is in control space (upper left corner origin)
    public Point ViewportCenterPoint { get; }
    //public Point CenterPoint { get; }

    //public Point GetCenterOffset( IMapProjection mapProjection )
    //{
    //    var projectionCenter = TileRegion.GetProjectionRect( mapProjection ).Center();
    //    //projectionCenter = mapProjection.ToUpperLeftOrigin( projectionCenter );

    //    return new Point( projectionCenter.X - ViewportCenterPoint.X, projectionCenter.Y - ViewportCenterPoint.Y );
    //}

    //public int HorizontalTiles { get; }
    //public int VerticalTiles { get; }
    //public int ZoomLevel { get; }

    //public double ProjectionWidth { get; }
    //public double ProjectionHeight { get; }

    //public double ControlWidth { get; }
    //public double ControlHeight { get; }

    public IEnumerable<MapTile> GetTileCoordinates( IMapProjection mapProjection )
    {
        for( var yTile = TileRegion.UpperLeft.Y; yTile <= TileRegion.LowerRight.Y; yTile++ )
        {
            for( var xTile = TileRegion.UpperLeft.X; xTile <= TileRegion.LowerRight.X; xTile++ )
            {
                yield return new MapTile( xTile, yTile, mapProjection.ZoomLevel );
            }
        }
    }
}