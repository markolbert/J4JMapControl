namespace J4JSoftware.MapLibrary;

public interface IMapSourceParameters
{
    double MaxLatitude { get; init; }
    int MaxDetailLevel { get; init; }
    int EarthRadius { get; init; }
}
