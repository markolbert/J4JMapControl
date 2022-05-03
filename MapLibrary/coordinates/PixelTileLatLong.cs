namespace J4JSoftware.MapLibrary;

public record PixelTileLatLong(
    DoublePoint PixelUpperLeft,
    IntPoint Tile,
    LatLong LatLong,
    IZoom Zoom
) : Coordinates( PixelUpperLeft, Zoom )
{
    public virtual bool Equals( PixelTileLatLong? other )
    {
        if( ReferenceEquals( null, other ) )
            return false;
        if( ReferenceEquals( this, other ) )
            return true;

        return base.Equals( other )
         && Tile.Equals( other.Tile )
         && LatLong.Equals( other.LatLong );
    }

    public override int GetHashCode() => HashCode.Combine( base.GetHashCode(), Tile, LatLong );
}