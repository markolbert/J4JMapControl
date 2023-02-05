namespace J4JMapLibrary;

public class MapScope : IMapScope
{
    #region IEqualityComparer...

    public bool Equals(IMapScope? x, IMapScope? y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (ReferenceEquals(x, null))
            return false;
        if (ReferenceEquals(y, null))
            return false;
        if (x.GetType() != y.GetType())
            return false;

        return x.LatitudeRange.Equals(y.LatitudeRange)
         && x.LongitudeRange.Equals(y.LongitudeRange);
    }

    public int GetHashCode(IMapScope obj)
    {
        return HashCode.Combine(obj.LatitudeRange, obj.LongitudeRange);
    }

    #endregion

    public static MapScope Copy(MapScope toCopy) => new(toCopy);

    public MapScope()
    {
        LatitudeRange = new MinMax<float>(-90, 90);
        LongitudeRange = new MinMax<float>(-180, 180);
    }

    protected MapScope( MapScope toCopy )
    {
        LatitudeRange = new MinMax<float>( toCopy.LatitudeRange.Maximum, toCopy.LatitudeRange.Maximum );
        LongitudeRange = new MinMax<float>( toCopy.LongitudeRange.Maximum, toCopy.LongitudeRange.Maximum );
    }

    public MinMax<float> LatitudeRange { get; internal set; } 
    public MinMax<float> LongitudeRange { get; internal set; }
}
