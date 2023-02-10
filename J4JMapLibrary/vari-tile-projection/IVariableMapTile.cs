namespace J4JMapLibrary;

public interface IVariableMapTile : IMapTile
{
    LatLong Center { get; }
    float Height { get; }
    float Width { get; }
    int Scale { get; }
}
