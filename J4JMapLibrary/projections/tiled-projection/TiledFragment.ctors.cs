namespace J4JMapLibrary;

public partial class TiledFragment : MapFragment
{
    // internal to hopefully avoid stack overflow
    internal TiledFragment(
        ITiledProjection projection,
        int xTile,
        int yTile,
        int scale,
        byte[] imageData
    )
        : this( projection, xTile, yTile, scale )
    {
        ImageData = imageData;
    }

    private TiledFragment(
        ITiledProjection projection,
        int xTile,
        int yTile,
        int scale
    )
        : base( projection )
    {
        TiledScale = projection.TiledScale;
        HeightWidth = projection.MapServer.TileHeightWidth;

        X = xTile < 0 ? 0 : xTile;
        Y = yTile < 0 ? 0 : yTile;
        QuadKey = this.GetQuadKey( scale );
    }
}
