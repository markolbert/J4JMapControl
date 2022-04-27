namespace J4JSoftware.MapLibrary;

public class ScreenPoint
{
    public event EventHandler? ValueChanged;

    private readonly LimitedPoint<int> _screenX;
    private readonly LimitedPoint<int> _screenY;

    public ScreenPoint()
    {
        _screenX = new LimitedPoint<int>( new IntLimits( 0, int.MaxValue ) );
        _screenY = new LimitedPoint<int>( new IntLimits( 0, int.MaxValue ) );
    }

    public int X => _screenX.Value;
    public int Y => _screenY.Value;

    public void Set( IntPoint point )
    {
        _screenX.Value = point.X;
        _screenY.Value = point.Y;

        OnValueChanged();
    }

    protected virtual void OnValueChanged() => ValueChanged?.Invoke( this, EventArgs.Empty );
}
