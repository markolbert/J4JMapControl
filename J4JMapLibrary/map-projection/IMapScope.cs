namespace J4JMapLibrary;

public interface IMapScope : IEquatable<MapScope>
{
    int Scale { get; }

    MinMax<int> ScaleRange { get; }
    MinMax<float> LatitudeRange { get; }
    MinMax<float> LongitudeRange { get; }
}
