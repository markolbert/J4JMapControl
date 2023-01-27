namespace J4JMapLibrary;

public record MinMax<T>
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
}
