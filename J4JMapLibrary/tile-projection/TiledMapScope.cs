namespace J4JMapLibrary;

public class TiledMapScope : MapScope, ITiledMapScope
{
 public static TiledMapScope Copy( TiledMapScope toCopy ) => new TiledMapScope( toCopy );

    public TiledMapScope()
    {
        ScaleRange = new MinMax<int>( 0, 0 );
        XRange = new MinMax<int>( int.MinValue, int.MaxValue );
        YRange = new MinMax<int>(int.MinValue, int.MaxValue);
    }

    private TiledMapScope( TiledMapScope toCopy )
        : base( toCopy )
    {
        Scale = toCopy.Scale;
        ScaleRange = new MinMax<int>( toCopy.ScaleRange.Minimum, toCopy.ScaleRange.Maximum );
        XRange = new MinMax<int>( toCopy.XRange.Minimum, toCopy.XRange.Maximum );
        YRange = new MinMax<int>( toCopy.YRange.Minimum, toCopy.YRange.Maximum );
    }

    public int Scale { get; internal set; }
    public MinMax<int> ScaleRange { get; internal set; }
    public MinMax<int> XRange { get; internal set; }
    public MinMax<int> YRange { get; internal set; }

    public bool Equals(TiledMapScope? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && Scale == other.Scale && ScaleRange.Equals(other.ScaleRange) && XRange.Equals(other.XRange) && YRange.Equals(other.YRange);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == this.GetType() && Equals((TiledMapScope)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Scale, ScaleRange, XRange, YRange);
    }

    public static bool operator ==(TiledMapScope? left, TiledMapScope? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TiledMapScope? left, TiledMapScope? right)
    {
        return !Equals(left, right);
    }
}
