namespace J4JMapLibrary;

public record MinMax<T>
    where T : struct, IComparable
{
    #region IEqualityComparer...

    private sealed class MinimumMaximumEqualityComparer : IEqualityComparer<MinMax<T>>
    {
        public bool Equals( MinMax<T>? x, MinMax<T>? y )
        {
            if( ReferenceEquals( x, y ) )
                return true;
            if( ReferenceEquals( x, null ) )
                return false;
            if( ReferenceEquals( y, null ) )
                return false;
            if( x.GetType() != y.GetType() )
                return false;

            return x.Minimum.Equals( y.Minimum )
             && x.Maximum.Equals( y.Maximum );
        }

        public int GetHashCode( MinMax<T> obj )
        {
            return HashCode.Combine( obj.Minimum, obj.Maximum );
        }
    }

    public static IEqualityComparer<MinMax<T>> DefaultComparer { get; } = new MinimumMaximumEqualityComparer();

    #endregion

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
}
