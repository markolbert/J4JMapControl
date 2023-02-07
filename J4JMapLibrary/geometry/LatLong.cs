namespace J4JMapLibrary;

public class LatLong
{
    public EventHandler? Changed;

    private readonly MinMax<float> _latitudeRange;
    private readonly MinMax<float> _longitudeRange;

    public LatLong(
        IMapScope scope
    )
    {
        _latitudeRange = scope.LatitudeRange;
        _longitudeRange = scope.LongitudeRange;

        Scale = scope.Scale;
    }

    public int Scale { get; internal set; }

    public float Latitude { get; private set; }
    public float Longitude { get; private set; }

    public void SetLatLong( float? latitude, float? longitude )
    {
        if( latitude == null && longitude == null )
            return;

        if( latitude.HasValue )
            Latitude = _latitudeRange.ConformValueToRange( latitude.Value, "Latitude" );

        if( longitude.HasValue )
            Longitude = _longitudeRange.ConformValueToRange(longitude.Value, "Longitude");

        Changed?.Invoke( this, EventArgs.Empty );
    }

    public void SetLatLong( LatLong latLong )
    {
        Latitude = _latitudeRange.ConformValueToRange(latLong.Latitude, "Latitude");
        Longitude = _longitudeRange.ConformValueToRange(latLong.Longitude, "Longitude");

        Changed?.Invoke( this, EventArgs.Empty );
    }
}
