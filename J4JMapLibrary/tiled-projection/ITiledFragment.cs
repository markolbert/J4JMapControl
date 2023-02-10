namespace J4JMapLibrary;

public interface ITiledFragment : IMapFragment
{
    int HeightWidth { get; }
    string QuadKey { get; }
    int X { get; }
    int Y { get; }
}
