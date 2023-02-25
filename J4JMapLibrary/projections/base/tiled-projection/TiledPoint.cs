namespace J4JSoftware.J4JMapLibrary;

public class TiledPoint : StaticPoint
{
    public TiledPoint(
        ITiledProjection projection
    )
        : base( projection )
    {
    }

    public int XTile => X / Projection.TileHeightWidth;
    public int YTile => Y / Projection.TileHeightWidth;
}
