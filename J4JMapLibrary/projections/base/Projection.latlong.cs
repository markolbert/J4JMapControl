namespace J4JSoftware.J4JMapLibrary;

public abstract partial class Projection
{
    public float MaxLatitude
    {
        get => _maxLat;

        protected set
        {
            _maxLat = value;
            LatitudeRange = new MinMax<float>( MinLatitude, MaxLatitude );
        }
    }

    public float MinLatitude
    {
        get => _minLat;

        protected set
        {
            _minLat = value;
            LatitudeRange = new MinMax<float>( MinLatitude, MaxLatitude );
        }
    }

    public MinMax<float> LatitudeRange { get; private set; }

    public float MaxLongitude
    {
        get => _maxLong;

        protected set
        {
            _maxLong = value;
            LongitudeRange = new MinMax<float>( MinLongitude, MaxLongitude );
        }
    }

    public float MinLongitude
    {
        get => _minLong;

        protected set
        {
            _minLong = value;
            LongitudeRange = new MinMax<float>( MinLongitude, MaxLongitude );
        }
    }

    public MinMax<float> LongitudeRange { get; private set; }
}
