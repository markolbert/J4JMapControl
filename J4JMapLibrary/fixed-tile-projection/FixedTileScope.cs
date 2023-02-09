namespace J4JMapLibrary;

public class FixedTileScope : MapScope, IFixedTileScope
{
    public static FixedTileScope Copy( FixedTileScope toCopy ) => new( toCopy );

    public FixedTileScope()
    {
        XRange = new MinMax<int>( int.MinValue, int.MaxValue );
        YRange = new MinMax<int>( int.MinValue, int.MaxValue );
    }

    private FixedTileScope( FixedTileScope toCopy )
        : base( toCopy )
    {
        XRange = new MinMax<int>( toCopy.XRange.Minimum, toCopy.XRange.Maximum );
        YRange = new MinMax<int>( toCopy.YRange.Minimum, toCopy.YRange.Maximum );
    }

    public MinMax<int> XRange { get; internal set; }
    public MinMax<int> YRange { get; internal set; }

    public bool Equals( FixedTileScope? other )
    {
        if( ReferenceEquals( null, other ) ) return false;
        if( ReferenceEquals( this, other ) ) return true;

        return base.Equals( other ) && XRange.Equals( other.XRange ) && YRange.Equals( other.YRange );
    }

    public override bool Equals( object? obj )
    {
        if( ReferenceEquals( null, obj ) ) return false;
        if( ReferenceEquals( this, obj ) ) return true;

        return obj.GetType() == GetType() && Equals( (FixedTileScope) obj );
    }

    public override int GetHashCode() => HashCode.Combine( base.GetHashCode(), XRange, YRange );

    public static bool operator==( FixedTileScope? left, FixedTileScope? right ) => Equals( left, right );

    public static bool operator!=( FixedTileScope? left, FixedTileScope? right ) => !Equals( left, right );
}
