namespace J4JMapLibrary;

public partial class FixedMapTile : MapTileBase<TileScope>
{
    protected override string TileId => $"{X}, {Y}";

    public int HeightWidth { get; }
    public string QuadKey { get; }
    public int X { get; }
    public int Y { get; }
}
