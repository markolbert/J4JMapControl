namespace J4JMapLibrary;

public interface IFixedTileScope : IMapScope, IEquatable<FixedTileScope>
{
    MinMax<int> XRange { get; }
    MinMax<int> YRange { get; }
}