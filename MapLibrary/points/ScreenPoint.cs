namespace J4JSoftware.MapLibrary;

public class ScreenPoint
{
    public event EventHandler? ValueChanged;

    private readonly LimitedPoint<double> _screenX;
    private readonly LimitedPoint<double> _screenY;

    public ScreenPoint()
    {
        _screenX = new LimitedPoint<double>( new DoubleLimits( 0, int.MaxValue ) );
        _screenY = new LimitedPoint<double>( new DoubleLimits( 0, int.MaxValue ) );
    }

    public double X => _screenX.Value;
    public double Y => _screenY.Value;

    public void Set( DoublePoint point )
    {
        _screenX.Value = point.X;
        _screenY.Value = point.Y;

        OnValueChanged();
    }

    protected virtual void OnValueChanged() => ValueChanged?.Invoke( this, EventArgs.Empty );
}
