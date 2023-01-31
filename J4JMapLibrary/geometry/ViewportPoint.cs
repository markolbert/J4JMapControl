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
        _height = height; 
        _width = width;

        LatLong.SetLatLong(latLong);
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

    public int Height => _height;
    public int Width => _width;

    public void SetHeightWidth(int? height, int? width)
    {
        if (height == null && width == null)
            return;

        // Metrics is about to change, but its XRange and YRange limits never do
        if (height.HasValue)
            _height = InternalExtensions.ConformValueToRange(height.Value, Metrics.YRange, "Y");

        if (width.HasValue)
            _width = InternalExtensions.ConformValueToRange(width.Value, Metrics.XRange, "X");

        Metrics = Metrics with
        {
            XRange = new MinMax<int>(0, _width),
            YRange = new MinMax<int>(0, _height)
        };

        Cartesian = Metrics.LatLongToCartesian(LatLong);
    }
}
