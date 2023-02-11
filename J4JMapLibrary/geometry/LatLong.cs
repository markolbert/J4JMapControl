namespace J4JMapLibrary;

public class LatLong
{
    private readonly MinMax<float> _latitudeRange;
    private readonly MinMax<float> _longitudeRange;
    public EventHandler? Changed;

    public LatLong(
        IMapServer server
    )
    {
        _latitudeRange = server.LatitudeRange;
        _longitudeRange = server.LongitudeRange;
    }

    public float Latitude { get; private set; }
    public float Longitude { get; private set; }

    public void SetLatLong( float? latitude, float? longitude )
    {
        if( latitude == null && longitude == null )
            return;

        if( latitude.HasValue )
            Latitude = _latitudeRange.ConformValueToRange( latitude.Value, "Latitude" );

        if( longitude.HasValue )
            Longitude = _longitudeRange.ConformValueToRange( longitude.Value, "Longitude" );

        Changed?.Invoke( this, EventArgs.Empty );
    }

    public void SetLatLong( LatLong latLong )
    {
        Latitude = _latitudeRange.ConformValueToRange( latLong.Latitude, "Latitude" );
        Longitude = _longitudeRange.ConformValueToRange( latLong.Longitude, "Longitude" );

        Changed?.Invoke( this, EventArgs.Empty );
    }
}
