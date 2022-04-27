namespace J4JSoftware.MapLibrary;

public record MapSourceParameters( double MaxLatitude, int MaxDetailLevel, int EarthRadius ) : IMapSourceParameters
{
    public static MapSourceParameters Default =
        new( GlobalConstants.Wgs84MaxLatitude,
             GlobalConstants.DefaultMaxDetailLevel,
             GlobalConstants.EarthRadius );
}
