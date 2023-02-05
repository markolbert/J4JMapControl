namespace J4JMapLibrary;

public class TiledMapScope : MapScope, ITiledMapScope
{
    #region IEqualityComparer...

    public bool Equals(ITiledMapScope? x, ITiledMapScope? y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (ReferenceEquals(x, null))
            return false;
        if (ReferenceEquals(y, null))
            return false;
        if (x.GetType() != y.GetType())
            return false;

        return x.Scale == y.Scale
         && x.ScaleRange.Equals(y.ScaleRange)
         && x.XRange.Equals(y.XRange)
         && x.YRange.Equals(y.YRange);
    }

    public int GetHashCode(ITiledMapScope obj)
    {
        return HashCode.Combine(obj.Scale, obj.ScaleRange, obj.XRange, obj.YRange);
    }

    #endregion

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
}
