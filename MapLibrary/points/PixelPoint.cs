namespace J4JSoftware.MapLibrary;

public class PixelPoint
{
    public static PixelPoint Empty { get; } = new PixelPoint();

    public event EventHandler? ValueChanged;

    private readonly LimitedPoint<double> _pixelX;
    private readonly LimitedPoint<double> _pixelY;

    public PixelPoint()
    {
        _pixelX = new LimitedPoint<double>( new DoubleLimits( 0, int.MaxValue ) );
        _pixelY = new LimitedPoint<double>( new DoubleLimits( 0, int.MaxValue ) );
    }

    public double X => _pixelX.Value;
    public double Y => _pixelY.Value;

    public void Set( DoublePoint point )
    {
        _pixelX.Value = point.X;
        _pixelY.Value = point.Y;

        OnValueChanged();
    }

    public DoublePoint ToDoublePoint()=>new( _pixelX.Value, _pixelY.Value );

    protected virtual void OnValueChanged() => ValueChanged?.Invoke( this, EventArgs.Empty );
}
