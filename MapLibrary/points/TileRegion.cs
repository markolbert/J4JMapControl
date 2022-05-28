using Windows.Foundation;

namespace J4JSoftware.MapLibrary;

public record TileRegion( MapTile UpperLeft, MapTile LowerRight )
{
    public int HorizontalTiles => LowerRight.X - UpperLeft.X + 1;
    public int VerticalTiles => LowerRight.Y - UpperLeft.Y + 1;

    public bool IsInRegion( MapTile tile )
    {
        if( tile.X < UpperLeft.X || tile.X > LowerRight.X )
            return false;

        if( tile.Y < UpperLeft.Y || tile.Y > LowerRight.Y )
            return false;

        return true;
    }

    public Point GetUpperLeft( IMapProjection mapProjection ) =>
        mapProjection.ToScreenPoint( UpperLeft );

    public Point GetLowerRight( IMapProjection mapProjection ) =>
        mapProjection.ToScreenPoint( LowerRight );

    // upper left corner origin
    public Rect GetProjectionRect( IMapProjection mapProjection ) =>
        new( GetUpperLeft( mapProjection ),
                  new Size( HorizontalTiles * mapProjection.TileWidthHeight,
                            VerticalTiles * mapProjection.TileWidthHeight ) );

    public LatLong GetCenterLatLong( IMapProjection mapProjection ) =>
        mapProjection.ScreenToLatLong( GetProjectionRect( mapProjection ).Center() );
}
