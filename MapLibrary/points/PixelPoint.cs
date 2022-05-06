namespace J4JSoftware.MapLibrary;

public class PixelPointBase
{
    public event EventHandler? ValueChanged;

    private readonly LimitedPoint<double> _pixelX;
    private readonly LimitedPoint<double> _pixelY;

    protected PixelPointBase( 
        IPointValueLimits<double> limits 
        )
    {
        Limits = limits;

        _pixelX = new LimitedPoint<double>( Limits );
        _pixelY = new LimitedPoint<double>( Limits );
    }

    protected PixelPointBase( PixelPointBase toCopy )
        : this( toCopy.Limits )
    {
        _pixelX.Value = toCopy.X;
        _pixelY.Value = toCopy.Y;
    }

    public PixelPointBase Copy() => new( this );

    protected IPointValueLimits<double> Limits { get; }

    public double X => _pixelX.Value;
    public double Y => _pixelY.Value;

    //public void Set( DoublePoint point )
    //{
    //    _pixelX.Value = point.X;
    //    _pixelY.Value = point.Y;

    //    OnValueChanged();
    //}

    public void Set( double x, double y )
    {
        _pixelX.Value = x;
        _pixelY.Value = y;

        OnValueChanged();
    }

    public DoublePoint ToDoublePoint()=>new( _pixelX.Value, _pixelY.Value );

    protected virtual void OnValueChanged() => ValueChanged?.Invoke( this, EventArgs.Empty );
}
