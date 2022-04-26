namespace J4JSoftware.MapLibrary;

public record BingMapsZoom( int Level ) : Zoom( Level, GlobalConstants.BingMapsMaxDetailLevel, new MapSourceParameters() )
{
    public override IntPoint ScreenToTile( IntPoint screenPoint ) =>
        new( screenPoint.X / 256, screenPoint.Y / 256 );

    public override IntPoint TileToScreen( IntPoint tilePoint ) => new( tilePoint.X * 256, tilePoint.Y * 256 );
}
