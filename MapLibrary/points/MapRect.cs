namespace J4JSoftware.MapLibrary;

public class MapRect
{
    public MapRect(
        IZoom zoom
    )
    {
        // we don't use MapRetrieverInfo, but the MapPoint ctor does
        if( zoom.MapRetrieverInfo == null )
            throw new ArgumentException(
                $"Attempting to create {typeof( MapRect )} with an undefined {nameof( MapRetrieverInfo )}" );

        UpperLeft = new MapPoint( zoom );
        LowerRight = new MapPoint( zoom );
    }

    public MapRect(
        MapPoint upperLeft,
        MapPoint lowerRight
    )
    {
        UpperLeft = upperLeft;
        LowerRight = lowerRight;
    }

    public MapPoint UpperLeft { get; }
    public MapPoint LowerRight { get; }
}
