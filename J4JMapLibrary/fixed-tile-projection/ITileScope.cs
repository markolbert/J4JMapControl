namespace J4JMapLibrary;

public interface ITileScope : IMapScope, IEquatable<TileScope>
{
    MinMax<int> XRange { get; }
    MinMax<int> YRange { get; }
}
