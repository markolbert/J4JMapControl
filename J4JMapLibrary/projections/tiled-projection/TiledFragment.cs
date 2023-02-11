namespace J4JMapLibrary;

public partial class TiledFragment : MapFragment, ITiledFragment
{
    protected override string FragmentId => $"{X}, {Y}, {TiledScale.Scale}";

    public ITiledScale TiledScale { get; }
    public int HeightWidth { get; }
    public string QuadKey { get; }
    public int X { get; }
    public int Y { get; }
}
