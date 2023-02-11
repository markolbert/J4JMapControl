namespace J4JMapLibrary;

public class NormalizedViewport : INormalizedViewport
{
    private readonly IProjection _projection;

    private float _latitude;
    private float _longitude;
    private int _scale;

    public NormalizedViewport(
        IProjection projection
    )
    {
        _projection = projection;
    }

    public float CenterLatitude
    {
        get => _latitude;
        set => _latitude = _projection.MapServer.LatitudeRange.ConformValueToRange( value, "Latitude" );
    }

    public float CenterLongitude
    {
        get => _longitude;
        set => _longitude = _projection.MapServer.LongitudeRange.ConformValueToRange( value, "Longitude" );
    }

    public float Height { get; set; }
    public float Width { get; set; }

    public int Scale
    {
        get => _scale;
        set => _scale = _projection.MapServer.ScaleRange.ConformValueToRange( value, "Scale" );
    }
}
