namespace J4JMapLibrary;

public class MapScope : IMapScope
{
    public static MapScope Copy(MapScope toCopy) => new(toCopy);

    public MapScope()
    {
        LatitudeRange = new MinMax<float>(-90, 90);
        LongitudeRange = new MinMax<float>(-180, 180);
    }

    protected MapScope( MapScope toCopy )
    {
        LatitudeRange = new MinMax<float>( toCopy.LatitudeRange.Minimum, toCopy.LatitudeRange.Maximum );
        LongitudeRange = new MinMax<float>( toCopy.LongitudeRange.Minimum, toCopy.LongitudeRange.Maximum );
    }

    public MinMax<float> LatitudeRange { get; internal set; } 
    public MinMax<float> LongitudeRange { get; internal set; }

    public bool Equals(MapScope? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return LatitudeRange.Equals(other.LatitudeRange) && LongitudeRange.Equals(other.LongitudeRange);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MapScope)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(LatitudeRange, LongitudeRange);
    }

    public static bool operator ==(MapScope? left, MapScope? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(MapScope? left, MapScope? right)
    {
        return !Equals(left, right);
    }
}
