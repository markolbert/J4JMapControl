namespace J4JSoftware.MapLibrary;

public record ScreenTileGlobalCoordinates(
    DoublePoint ScreenUpperLeft,
    IntPoint TileCoordinates,
    LatLong GlobeCoordinates,
    IZoom Zoom
) : Coordinates( ScreenUpperLeft, Zoom )
{
    public virtual bool Equals( ScreenTileGlobalCoordinates? other )
    {
        if( ReferenceEquals( null, other ) )
            return false;
        if( ReferenceEquals( this, other ) )
            return true;

        return base.Equals( other )
         && TileCoordinates.Equals( other.TileCoordinates )
         && GlobeCoordinates.Equals( other.GlobeCoordinates );
    }

    public override int GetHashCode() => HashCode.Combine( base.GetHashCode(), TileCoordinates, GlobeCoordinates );
}