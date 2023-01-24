using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class MapPoint
{
    private readonly IJ4JLogger _logger;

    private double _latitude;
    private double _longitude;
    private int _x;
    private int _y;
    private int _horizontal;
    private int _vertical;

    internal MapPoint(
        ITiledProjection projection,
        IJ4JLogger logger
    )
    {
        Projection = projection;

        _logger = logger;
        _logger.SetLoggedType(GetType());
    }

    public ITiledProjection Projection { get; }

    internal void ChangeScale()
    {
        UpdateXY();
        UpdateHV();
    }
    
    public double Latitude
    {
        get => _latitude;
        set =>
            SetValue( ref _latitude,
                          value,
                          Projection.MinLatitude,
                          Projection.MaxLatitude,
                          "Latitude",
                          UpdateXY,
                          UpdateHV );
    }

    public double Longitude
    {
        get => _longitude;
        set => SetValue( ref _longitude, value, Projection.MinLongitude, Projection.MaxLongitude, "Longitude" );
    }

    public int X
    {
        get => _x;
        set => SetValue( ref _x, value, Projection.MinX, Projection.MaxX, "X", UpdateLatLong, UpdateHV );
    }

    public int Y
    {
        get => _y;
        set => SetValue(ref _y, value, Projection.MinY, Projection.MinY, "Y", UpdateLatLong, UpdateHV );
    }

    public int Horizontal
    {
        get => _horizontal;
        set => SetValue(ref _horizontal, value, 0, 0, "Horizontal", UpdateXY, UpdateLatLong);
    }

    public int Vertical
    {
        get => _vertical;
        set => SetValue(ref _vertical, value, 0, int.MaxValue, "Vertical", UpdateXY, UpdateLatLong);
    }

    // ReSharper disable once RedundantAssignment
    private void SetValue(ref double target, double value, double min, double max, string propName, params Action[] updaters)
    {
        target = value;

        if (value < min)
        {
            _logger.Warning<string, double, double>("{0} value ({1}) less than projection minimum ({2}), capping",
                                                   propName,
                                                   value,
                                                   min);
            target = min;
        }
        else
        {
            if (value > max)
            {
                _logger.Warning<string, double, double>("{0} value ({1}) greater than projection maximum ({2}), capping",
                                                       propName,
                                                       value,
                                                       max);

                target = max;
            }
        }

        foreach (var updater in updaters)
        {
            updater();
        }
    }

    // ReSharper disable once RedundantAssignment
    private void SetValue(ref int target, int value, int min, int max, string propName, params Action[] updaters)
    {
        target = value;

        if (value < min)
        {
            _logger.Warning<string, int, int>("{0} value ({1}) less than projection minimum ({2}), capping",
                                             propName,
                                             value,
                                             min);
            target = min;
        }

        if (value > max)
        {
            _logger.Warning<string, int, int>("{0} value ({1}) greater than projection maximum ({2}), capping",
                                             propName,
                                             value,
                                             max);

            target = max;
        }

        foreach (var updater in updaters)
        {
            updater();
        }
    }

    // X & Y are presumed to be valid if this method is called
    private void UpdateLatLong()
    {
        var latLong = Projection.XYToLatLong( _x, _y );

        _latitude = latLong.latitude;
        _longitude = latLong.longitude;
    }

    // Latitude & Longitude are presumed to be valid if this method is called
    private void UpdateXY()
    {
        var xy = Projection.LatLongToXY(_latitude, _longitude);

        _x = xy.x;
        _y = xy.y;
    }

    // X & Y are presumed to be valid if this method is called
    private void UpdateHV()
    {
        _horizontal = Projection.MinX - _x;
        _vertical = Projection.MaxY - _y;
    }
}