namespace J4JMapLibrary;

public class MapPoint
{
    public MapPoint(
        ProjectionMetrics metrics
    )
    {
        Metrics = metrics;
        Scale = metrics.Scale;

        LatLong = new LatLong( metrics );
        Cartesian = new Cartesian( metrics );
    }

    public ProjectionMetrics Metrics { get; }

    public LatLong LatLong { get; }
    public Cartesian Cartesian { get; }
    public int Scale { get; }
    public bool ReflectsProjection => Scale == Metrics.Scale;
}
