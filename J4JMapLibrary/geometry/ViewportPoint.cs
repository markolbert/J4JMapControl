namespace J4JMapLibrary;

public class ViewportPoint : MapPoint
{
    private int _height;
    private int _width;

    public ViewportPoint(
        int height,
        int width,
        LatLong latLong
    )
        :base(CreateMetrics(height, width))
    {
        Height = height; 
        Width = width;

        LatLong.Latitude = latLong.Latitude;
        LatLong.Longitude = latLong.Longitude;
    }

    private static ProjectionMetrics CreateMetrics(int height, int width)
    {
        return new ProjectionMetrics()
        {
            LatitudeRange =
                new MinMax<double>(-MapConstants.MaxMercatorLatitude, MapConstants.MaxMercatorLatitude),
            LongitudeRange = new MinMax<double>(-180, 180),
            Scale = 1,
            ScaleRange = new MinMax<int>(1, 1),
            TileXRange = MapConstants.ZeroInt,
            TileYRange = MapConstants.ZeroInt,
            XRange = new MinMax<int>(0, width),
            YRange = new MinMax<int>(0, height)
        };
    }

    public int Height
    {
        get => _height;

        set
        {
            if( value <= 0 )
            {
                Logger?.Error("Trying to set ViewportPoint height to <= 0 value, ignoring");
                return;
            }

            _height = value;

            Metrics = Metrics with { XRange = new MinMax<int>( 0, _width ), YRange = new MinMax<int>( 0, _height ) };
            Cartesian = Metrics.LatLongToCartesian( LatLong );
        }
    }

    public int Width
    {
        get => _width;

        set
        {
            if (value <= 0)
            {
                Logger?.Error("Trying to set ViewportPoint width to <= 0 value, ignoring");
                return;
            }

            _width = value;

            Metrics = Metrics with { XRange = new MinMax<int>(0, _width), YRange = new MinMax<int>(0, _height) };
            Cartesian = Metrics.LatLongToCartesian(LatLong);
        }
    }
}
