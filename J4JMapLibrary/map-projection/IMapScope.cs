namespace J4JMapLibrary;

public interface IMapScope : IEquatable<MapScope>
{
    MinMax<float> LatitudeRange { get; }
    MinMax<float> LongitudeRange { get; }
}
