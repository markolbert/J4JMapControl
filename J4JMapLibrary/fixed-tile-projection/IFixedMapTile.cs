namespace J4JMapLibrary;

public interface IFixedMapTile : IMapTile
{
    int HeightWidth { get; }
    string QuadKey { get; }
    int X { get; }
    int Y { get; }
}
