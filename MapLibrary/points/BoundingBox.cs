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

            var desiredCenterPt = MapProjection.LatLongToScreen(ViewportCenter);
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

            _tileRegion = MapProjection.GetTileRegion( ViewportCenter, Viewport.Width, Viewport.Height, Rotation );
            return _tileRegion;
        }
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