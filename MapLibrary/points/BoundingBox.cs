using System.Collections;
using System.Collections.Generic;
using Windows.Foundation;

namespace J4JSoftware.MapLibrary;

public record BoundingBox(
    IMapProjection MapProjection,
    Rect Viewport,
    LatLong ViewportCenter,
    double Rotation
) : IEnumerable<MapTile>
{
    private Point? _vpCenterPt;
    private TileRegion? _tileRegion;

    public Point ViewportCenterPoint
    {
        get
        {
            if( _vpCenterPt != null )
                return _vpCenterPt.Value;

            var desiredCenterPt = MapProjection.LatLongToCartesian(ViewportCenter);
            _vpCenterPt = MapProjection.ToUpperLeftOrigin( desiredCenterPt );

            return _vpCenterPt.Value;
        }
    }

    public TileRegion TileRegion
    {
        get
        {
            if( _tileRegion != null )
                return _tileRegion;

            _tileRegion = MapProjection.GetTileRegion( this );
            return _tileRegion;
        }
    }

    public Point GetCenterOffset()
    {
        var projectionCenter = TileRegion.GetProjectionRect(MapProjection).Center();

        return new Point(projectionCenter.X - ViewportCenterPoint.X,
                         projectionCenter.Y - ViewportCenterPoint.Y);
    }

    public Point GetOffset()
    {
        // there are two offsets to consider:
        // - the difference between the desired center of the viewport and the actual
        //   center of the tiles that were retrieved
        // - the difference between the size/width of the viewport and the size/width of the tiles
        //   that were retrieved
        var mapOffset = GetCenterOffset();

        var vpOffsetX = (Viewport.Width - TileRegion.HorizontalTiles * MapProjection.TileWidthHeight)
          / 2;
        var vpOffsetY = (Viewport.Height - TileRegion.VerticalTiles * MapProjection.TileWidthHeight)
          / 2;

        // if we're displaying everything available horizontally the offset is 0.0
        return TileRegion.HorizontalTiles == MapProjection.ZoomFactor
            ? new Point(0.0, 0.0)
            : new Point(vpOffsetX + mapOffset.X, vpOffsetY + mapOffset.Y);
    }

    public IEnumerator<MapTile> GetEnumerator()
    {
        for( var yTile = TileRegion.UpperLeft.Y; yTile <= TileRegion.LowerRight.Y; yTile++ )
        {
            for( var xTile = TileRegion.UpperLeft.X; xTile <= TileRegion.LowerRight.X; xTile++ )
            {
                yield return new MapTile( xTile, yTile, MapProjection.ZoomLevel );
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}