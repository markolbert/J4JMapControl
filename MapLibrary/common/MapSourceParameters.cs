namespace J4JSoftware.MapLibrary;

public record MapSourceParameters(
    double MaxLatitude = GlobalConstants.Wgs84MaxLatitude,
    int MaxDetailLevel = GlobalConstants.MaxDetailLevel,
    int EarthRadius = GlobalConstants.EarthRadius
) : IMapSourceParameters;
