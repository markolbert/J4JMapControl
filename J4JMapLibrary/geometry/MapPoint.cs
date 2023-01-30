using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

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

        Logger = J4JDeusEx.GetLogger();
        Logger?.SetLoggedType( GetType() );
    }

    protected IJ4JLogger? Logger { get; }

    public ProjectionMetrics Metrics { get; protected set; }

    public LatLong LatLong { get; }
    public Cartesian Cartesian { get; protected set; }
    public int Scale { get; }
    public bool ReflectsProjection => Scale == Metrics.Scale;
}
