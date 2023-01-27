using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class LatLong
{
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

    public double Latitude
    {
        get => _latitude;

        set =>
            _latitude = InternalExtensions.ConformValueToRange( value,
                                                                Metrics.LatitudeRange,
                                                                "Latitude" );
    }

    public double Longitude
    {
        get => _longitude;

        set =>
            _longitude =
                InternalExtensions.ConformValueToRange( value,
                                                        Metrics.LongitudeRange,
                                                        "Longitude" );
    }
}
