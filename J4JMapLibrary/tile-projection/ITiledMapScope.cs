namespace J4JMapLibrary;

public interface ITiledMapScope : IMapScope, IEquatable<TiledMapScope>
{
    int Scale { get; }

    MinMax<int> ScaleRange { get; }
    MinMax<int> XRange { get; }
    MinMax<int> YRange { get; }
}