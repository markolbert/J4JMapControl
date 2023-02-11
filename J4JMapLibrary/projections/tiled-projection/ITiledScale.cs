namespace J4JMapLibrary;

public interface ITiledScale : IProjectionScale, IEquatable<TiledScale>
{
    MinMax<int> XRange { get; }
    MinMax<int> YRange { get; }
}
