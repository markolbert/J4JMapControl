namespace J4JMapLibrary;

public class MinMax<T> : IEquatable<MinMax<T>>
    where T : struct, IComparable
{
    public MinMax(
        T min,
        T max
    )
    {
        if( min.CompareTo( max ) > 0 )
        {
            // min/max are reversed
            Minimum = max;
            Maximum = min;
        }
        else
        {
            // min/max are in correct sequence
            Minimum = min;
            Maximum = max;
        }
    }

    public T Minimum { get; }
    public T Maximum { get; }

    public bool Equals( MinMax<T>? other )
    {
        if( ReferenceEquals( null, other ) ) return false;
        if( ReferenceEquals( this, other ) ) return true;

        return Minimum.Equals( other.Minimum ) && Maximum.Equals( other.Maximum );
    }

    public override bool Equals( object? obj )
    {
        if( ReferenceEquals( null, obj ) ) return false;
        if( ReferenceEquals( this, obj ) ) return true;
        if( obj.GetType() != this.GetType() ) return false;

        return Equals( (MinMax<T>) obj );
    }

    public override int GetHashCode()
    {
        return HashCode.Combine( Minimum, Maximum );
    }

    public static bool operator==( MinMax<T>? left, MinMax<T>? right )
    {
        return Equals( left, right );
    }

    public static bool operator!=( MinMax<T>? left, MinMax<T>? right )
    {
        return !Equals( left, right );
    }
}
