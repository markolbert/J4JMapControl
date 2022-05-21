namespace J4JSoftware.MapLibrary;

public record DoublePoint
{
    private readonly IMapProjection _mapProjection;
    private double _x;
    private double _y;

    public DoublePoint(
        CoordinateOrigin origin,
        IMapProjection mapProjection
    )
        : this( 0.0, 0.0, origin, mapProjection )
    {
    }

    public DoublePoint(
        double x,
        double y,
        CoordinateOrigin origin,
        IMapProjection mapProjection
    )
    {
        _x = x;
        _y = y;
        Origin = origin;
        _mapProjection = mapProjection;
    }

    public (double x, double y) GetValues( CoordinateOrigin origin ) =>
        origin == Origin
            ? ( _x, _y )
            : ( _mapProjection.ChangeOrigin( _x, CoordinateAxis.XAxis ),
                _mapProjection.ChangeOrigin( _y, CoordinateAxis.YAxis ) );

    //public double GetX( CoordinateOrigin origin ) =>
    //    origin == Origin ? _x : _mapProjection.ChangeOrigin( _x, CoordinateAxis.XAxis );

    //public void SetX( double value, CoordinateOrigin origin )
    //{
    //    if( origin != Origin )
    //        value = _mapProjection.ChangeOrigin( value, CoordinateAxis.XAxis );

    //    _x = value;
    //}

    public void Set( double xValue, double yValue, CoordinateOrigin origin )
    {
        if( origin == Origin )
        {
            _x = xValue;
            _y = yValue;
        }
        else
        {
            _x = _mapProjection.ChangeOrigin( xValue, CoordinateAxis.XAxis );
            _y = _mapProjection.ChangeOrigin( yValue, CoordinateAxis.YAxis );
        }
    }

    public void Increment( double xIncr, double yIncr )
    {
        _x += xIncr;
        _y += yIncr;
    }

    //public void IncrementX( double value ) => _x += value;

    //public double GetY( CoordinateOrigin origin ) =>
    //    origin == Origin ? _y : _mapProjection.ChangeOrigin( _y, CoordinateAxis.YAxis );

    //public void SetY( double value, CoordinateOrigin origin )
    //{
    //    if( origin != Origin )
    //        value = _mapProjection.ChangeOrigin( value, CoordinateAxis.YAxis );

    //    _y = value;
    //}

    //public void IncrementY( double value ) => _y += value;

    public CoordinateOrigin Origin { get; }
}
