namespace J4JMapLibrary;

public class LatLong
{
    public EventHandler? Changed;

    private readonly MinMax<float> _latitudeRange;
    private readonly MinMax<float> _longitudeRange;

    public LatLong(
        ProjectionMetrics metrics
    )
    {
        _latitudeRange = metrics.LatitudeRange;
        _longitudeRange = metrics.LongitudeRange;

        Scale = metrics.Scale;
    }

    public int Scale { get; internal set; }

    public float Latitude { get; private set; }
    public float Longitude { get; private set; }

    public void SetLatLong( float? latitude, float? longitude )
    {
        if( latitude == null && longitude == null )
            return;

        if( latitude.HasValue )
            Latitude = InternalExtensions
               .ConformValueToRange( latitude.Value, _latitudeRange, "Latitude" );

        if( longitude.HasValue )
            Longitude = InternalExtensions
               .ConformValueToRange( longitude.Value, _longitudeRange, "Longitude" );

        Changed?.Invoke( this, EventArgs.Empty );
    }

    public void SetLatLong( LatLong latLong )
    {
        Latitude = InternalExtensions
           .ConformValueToRange( latLong.Latitude, _latitudeRange, "Latitude" );

        Longitude = InternalExtensions
           .ConformValueToRange( latLong.Longitude, _longitudeRange, "Longitude" );

        Changed?.Invoke( this, EventArgs.Empty );
    }
}
