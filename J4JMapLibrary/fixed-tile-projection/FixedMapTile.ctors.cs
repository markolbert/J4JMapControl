namespace J4JMapLibrary;

public partial class FixedMapTile
{
    // internal to hopefully avoid stack overflow
    internal FixedMapTile(
        IFixedTileProjection projection,
        int xTile,
        int yTile,
        byte[] imageData
    )
        : this( projection, xTile, yTile )
    {
        ImageData = imageData;
    }

    private FixedMapTile(
        IFixedTileProjection projection,
        int xTile,
        int yTile
    )
        : base( projection )
    {
        HeightWidth = projection.MapServer.TileHeightWidth;

        X = xTile < 0 ? 0 : xTile;
        Y = yTile < 0 ? 0 : yTile;
        QuadKey = this.GetQuadKey();
    }
}
