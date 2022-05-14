namespace J4JSoftware.MapLibrary;

public record TilePoint( int X, int Y, int Z )
{
    private const double Tolerance = 0.01;

    #region IEquality

    public virtual bool Equals( TilePoint? other )
    {
        if( ReferenceEquals( null, other ) )
            return false;
        if( ReferenceEquals( this, other ) )
            return true;

        return X == other.X
         && Y == other.Y
         && Z == other.Z;
    }

    public override int GetHashCode() => HashCode.Combine( X, Y, Z );

    #endregion

    public TilePoint(
        double x,
        double y,
        int z
    )
        : this( RoundDouble(x), RoundDouble(y), z )
    {
    }

    private static int RoundDouble( double toRound )
    {
        var floor = Math.Floor( toRound );
        var retVal = Convert.ToInt32( floor );

        if( toRound - floor > Tolerance )
            retVal++;

        return retVal;
    }
}