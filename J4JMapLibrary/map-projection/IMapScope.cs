namespace J4JMapLibrary;

public interface IMapScope : IEqualityComparer<IMapScope>
{
    MinMax<float> LatitudeRange { get; }
    MinMax<float> LongitudeRange { get; }
}
