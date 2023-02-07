namespace J4JMapLibrary;

public class MapScope : IMapScope
{
    public static MapScope Copy(MapScope toCopy) => new(toCopy);

    public MapScope()
    {
        ScaleRange = new MinMax<int>(0, 0);
        LatitudeRange = new MinMax<float>(-90, 90);
        LongitudeRange = new MinMax<float>(-180, 180);
    }

    protected MapScope( MapScope toCopy )
    {
        Scale = toCopy.Scale;
        ScaleRange = new MinMax<int>(toCopy.ScaleRange.Minimum, toCopy.ScaleRange.Maximum);
        
        LatitudeRange = new MinMax<float>( toCopy.LatitudeRange.Minimum, toCopy.LatitudeRange.Maximum );
        LongitudeRange = new MinMax<float>( toCopy.LongitudeRange.Minimum, toCopy.LongitudeRange.Maximum );
    }

    public MinMax<float> LatitudeRange { get; internal set; } 
    public MinMax<float> LongitudeRange { get; internal set; }

    public int Scale { get; internal set; }
    public MinMax<int> ScaleRange { get; internal set; }

    public bool Equals(MapScope? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return LatitudeRange.Equals(other.LatitudeRange) && LongitudeRange.Equals(other.LongitudeRange) &&
               Scale == other.Scale && ScaleRange.Equals(other.ScaleRange);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == this.GetType() && Equals((MapScope)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(LatitudeRange, LongitudeRange, Scale, ScaleRange);
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
