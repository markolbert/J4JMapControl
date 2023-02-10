namespace J4JMapLibrary;

public interface ITiledScope : IMapScope, IEquatable<TiledScope>
{
    MinMax<int> XRange { get; }
    MinMax<int> YRange { get; }
}
