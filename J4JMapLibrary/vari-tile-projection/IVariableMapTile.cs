namespace J4JMapLibrary;

public interface IVariableMapTile : IMapTile
{
    LatLong Center { get; }
    int Height { get; }
    int Width { get; }
    int Scale { get; }
}
