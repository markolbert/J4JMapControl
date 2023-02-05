namespace J4JMapLibrary;

public interface ITiledMapScope : IMapScope, IEqualityComparer<ITiledMapScope>
{
    int Scale { get; }

    MinMax<int> ScaleRange { get; }
    MinMax<int> XRange { get; }
    MinMax<int> YRange { get; }
}