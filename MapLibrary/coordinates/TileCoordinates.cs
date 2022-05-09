namespace J4JSoftware.MapLibrary;

public record TileCoordinates(
    TilePoint Tile,
    IMapProjection MapProjection
) : Coordinates( MapProjection )
{
    public virtual bool Equals( TileCoordinates? other )
    {
        if( ReferenceEquals( null, other ) )
            return false;
        if( ReferenceEquals( this, other ) )
            return true;

        return base.Equals( other )
         && Tile.Equals( other.Tile );
    }

    public override int GetHashCode() => HashCode.Combine( base.GetHashCode(), Tile );
}