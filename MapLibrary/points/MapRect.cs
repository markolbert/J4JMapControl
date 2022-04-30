namespace J4JSoftware.MapLibrary;

public class MapRect
{
    public MapRect(
        IZoom zoom
    )
    {
        if( zoom.MapRetrieverInfo == null )
            throw new ArgumentException(
                $"Attempting to create {typeof( MapRect )} with an undefined {nameof( MapRetrieverInfo )}" );

        UpperLeft = new MapPoint( zoom );
        LowerRight = new MapPoint( zoom );
    }

    public MapPoint UpperLeft { get; }
    public MapPoint LowerRight { get; }
}
