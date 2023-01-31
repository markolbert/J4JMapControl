namespace J4JMapLibrary;

public class LatLong
{
    public EventHandler? Changed;

    private double _latitude;
    private double _longitude;

    public LatLong(
        ProjectionMetrics metrics
    )
    {
        Metrics = metrics;
        Scale = metrics.Scale;
    }

    public ProjectionMetrics Metrics { get; }
    public int Scale { get; }
    public bool ReflectsProjection => Scale == Metrics.Scale;

    public double Latitude => _latitude;
    public double Longitude => _longitude;

    public void SetLatLong(double? latitude, double? longitude)
    {
        if (latitude == null && longitude == null)
            return;

        if (latitude.HasValue)
            _latitude = InternalExtensions.ConformValueToRange(latitude.Value,
                Metrics.LatitudeRange,
                "Latitude");

        if (longitude.HasValue)
            _longitude = InternalExtensions.ConformValueToRange(longitude.Value,
                Metrics.LongitudeRange,
                "Longitude");

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetLatLong(LatLong latLong)
    {
        _latitude = InternalExtensions.ConformValueToRange(latLong.Latitude,
            Metrics.LatitudeRange,
            "Latitude");

        _longitude = InternalExtensions.ConformValueToRange(latLong.Longitude,
            Metrics.LongitudeRange,
            "Longitude");

        Changed?.Invoke(this, EventArgs.Empty);
    }
}
