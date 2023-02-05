using J4JMapLibrary.Viewport;

namespace J4JMapLibrary;

public static class ViewportRectangleExtensions
{
    public static ViewportRectangle Projection(this ViewportRectangle rectangle, ITiledProjection value)
    {
        rectangle.Projection = value;
        return rectangle;
    }

    public static ViewportRectangle Center(this ViewportRectangle rectangle, LatLong center)
    {
        rectangle.CenterLatitude = center.Latitude;
        rectangle.CenterLongitude = center.Longitude;

        return rectangle;
    }

    public static ViewportRectangle Center( this ViewportRectangle rectangle, float latitude, float longitude )
    {
        rectangle.CenterLatitude = latitude;
        rectangle.CenterLongitude = longitude;

        return rectangle;
    }

    public static ViewportRectangle Height(this ViewportRectangle rectangle, float value)
    {
        rectangle.Height = value;
        return rectangle;
    }

    public static ViewportRectangle Width(this ViewportRectangle rectangle, float value)
    {
        rectangle.Width = value;
        return rectangle;
    }

    public static ViewportRectangle HeightWidth(this ViewportRectangle rectangle, float height, float width)
    {
        rectangle.Height = height;
        rectangle.Width = width;

        return rectangle;
    }

    public static ViewportRectangle Heading(this ViewportRectangle rectangle, float value)
    {
        rectangle.Heading = value;
        return rectangle;
    }

}
