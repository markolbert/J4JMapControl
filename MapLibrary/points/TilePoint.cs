namespace J4JSoftware.MapLibrary;

public class TilePoint
{
    public event EventHandler? ValueChanged;

    private readonly IPointValueLimits<int> _limits;
    private readonly LimitedPoint<int> _tileX;
    private readonly LimitedPoint<int> _tileY;

    public TilePoint(
        IPointValueLimits<int> limits
    )
    {
        _limits = limits;

        _tileX = new LimitedPoint<int>( limits );
        _tileY = new LimitedPoint<int>( limits );
    }

    private TilePoint( TilePoint toCopy )
        : this( toCopy._limits )
    {
        _tileX.Value = X;
        _tileY.Value = Y;
    }

    public TilePoint Copy() => new( this );

    public int X => _tileX.Value;
    public int Y => _tileY.Value;

    public void Set( IntPoint point )
    {
        _tileX.Value = point.X;
        _tileY.Value = point.Y;

        OnValueChanged();
    }

    public IntPoint ToIntPoint() => new( _tileX.Value, _tileY.Value );

    protected virtual void OnValueChanged() => ValueChanged?.Invoke( this, EventArgs.Empty );
}
