namespace J4JSoftware.MapLibrary;

public class TilePoint
{
    public event EventHandler? ValueChanged;

    private readonly LimitedPoint<int> _tileX;
    private readonly LimitedPoint<int> _tileY;

    public TilePoint(
        IPointValueLimits<int> limits
    )
    {
        _tileX = new LimitedPoint<int>( limits );
        _tileY = new LimitedPoint<int>( limits );
    }

    public int X => _tileX.Value;
    public int Y => _tileY.Value;

    public void Set( IntPoint point )
    {
        _tileX.Value = point.X;
        _tileY.Value = point.Y;

        OnValueChanged();
    }

    protected virtual void OnValueChanged() => ValueChanged?.Invoke( this, EventArgs.Empty );
}
