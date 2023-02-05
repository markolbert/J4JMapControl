namespace J4JMapLibrary;

public interface IProjectionScope
{
    int Scale { get; set; }

    MinMax<int> ScaleRange { get; }
    MinMax<int> XRange { get; }
    MinMax<int> YRange { get; }
    MinMax<float> LatitudeRange { get; }
    MinMax<float> LongitudeRange { get; }
}
