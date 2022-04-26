namespace J4JSoftware.MapLibrary;

public record LatLong
{
    public LatLong(
        double latitude,
        double longitude,
        IMapSourceParameters mapSource
    )
    {
        MapSourceParameters = mapSource;

        Latitude = Math.Abs( latitude ) > MapSourceParameters.MaxLatitude
            ? MapSourceParameters.MaxLatitude * Math.Sign( latitude )
            : latitude;

        Longitude = Math.Abs( longitude ) > 180 ? 180 * Math.Sign( latitude ) : longitude;
    }

    protected IMapSourceParameters MapSourceParameters { get; }

    public double Latitude { get; }
    public double Longitude { get; }
}
