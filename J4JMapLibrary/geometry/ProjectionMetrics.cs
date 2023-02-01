namespace J4JMapLibrary;

public record ProjectionMetrics
{
    public MinMax<float> LatitudeRange { get; init; } = MapConstants.ZeroFloat;
    public MinMax<float> LongitudeRange { get; init; } = MapConstants.ZeroFloat;
    public MinMax<int> XRange { get; init; } = MapConstants.ZeroInt;
    public MinMax<int> YRange { get; init; } = MapConstants.ZeroInt;
    public MinMax<int> ScaleRange { get; init; } = MapConstants.ZeroInt;
    public MinMax<int> TileXRange { get; init; } = MapConstants.ZeroInt;
    public MinMax<int> TileYRange { get; init; } = MapConstants.ZeroInt;

    public int Scale { get; init; }
}
