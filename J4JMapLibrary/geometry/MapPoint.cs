using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class MapPoint
{
    public EventHandler? Changed;

    private int _changeEventsReceived;

    public MapPoint(
        ProjectionMetrics metrics
    )
    {
        Metrics = metrics;
        Scale = metrics.Scale;

        LatLong = new LatLong( metrics );
        LatLong.Changed += LatLongChanged;

        Cartesian = new Cartesian( metrics );
        Cartesian.Changed += CartesianChanged;

        Logger = J4JDeusEx.GetLogger();
        Logger?.SetLoggedType( GetType() );
    }

    private void CartesianChanged(object? sender, EventArgs e)
    {
        // LatLong and Cartesian events come in pairs, and we only
        // want to propagate the second one
        TrackChangeEvents();

        LatLong.SetLatLong(Metrics.CartesianToLatLong(Cartesian));
    }

    private void TrackChangeEvents()
    {
        _changeEventsReceived++;

        if (_changeEventsReceived < 2) 
            return;

        OnPositionChanged();
        _changeEventsReceived = 0;
    }

    private void LatLongChanged(object? sender, EventArgs e)
    {
        // LatLong and Cartesian events come in pairs, and we only
        // want to propagate the second one
        TrackChangeEvents();

        Cartesian.SetCartesian(Metrics.LatLongToCartesian(LatLong));
    }

    protected virtual void OnPositionChanged() => Changed?.Invoke(this, EventArgs.Empty);

    protected IJ4JLogger? Logger { get; }

    public ProjectionMetrics Metrics { get; protected set; }

    public LatLong LatLong { get; }
    public Cartesian Cartesian { get; protected set; }
    public int Scale { get; }
    public bool ReflectsProjection => Scale == Metrics.Scale;
}
