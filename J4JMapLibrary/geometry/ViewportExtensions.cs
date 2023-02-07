using J4JMapLibrary.Viewport;

namespace J4JMapLibrary;

public static class ViewportExtensions
{
    public static Viewport.Viewport Projection(this Viewport.Viewport rectangle, IFixedTileProjection value)
    {
        rectangle.Projection = value;
        return rectangle;
    }

    public static Viewport.Viewport Center(this Viewport.Viewport rectangle, LatLong center)
    {
        rectangle.CenterLatitude = center.Latitude;
        rectangle.CenterLongitude = center.Longitude;

        return rectangle;
    }

    public static Viewport.Viewport Center( this Viewport.Viewport rectangle, float latitude, float longitude )
    {
        rectangle.CenterLatitude = latitude;
        rectangle.CenterLongitude = longitude;

        return rectangle;
    }

    public static Viewport.Viewport Height(this Viewport.Viewport rectangle, float value)
    {
        rectangle.Height = value;
        return rectangle;
    }

    public static Viewport.Viewport Width(this Viewport.Viewport rectangle, float value)
    {
        rectangle.Width = value;
        return rectangle;
    }

    public static Viewport.Viewport HeightWidth(this Viewport.Viewport rectangle, float height, float width)
    {
        rectangle.Height = height;
        rectangle.Width = width;

        return rectangle;
    }

    public static Viewport.Viewport Heading(this Viewport.Viewport rectangle, float value)
    {
        rectangle.Heading = value;
        return rectangle;
    }

}
