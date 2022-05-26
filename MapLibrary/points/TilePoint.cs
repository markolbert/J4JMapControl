namespace J4JSoftware.MapLibrary;

public record TilePoint( int X, int Y, int Z)
{
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

    public override int GetHashCode()
    {
        return HashCode.Combine( X, Y, Z );
    }

    #endregion
}