namespace J4JSoftware.J4JMapLibrary;

public static class MapRegionFluentExtensions
{
    public static MapRegion.MapRegion CenterLatitude( this MapRegion.MapRegion region, float value )
    {
        region.CenterLatitude = value;
        return region;
    }

    public static MapRegion.MapRegion CenterLongitude( this MapRegion.MapRegion region, float value )
    {
        region.CenterLongitude = value;
        return region;
    }

    public static MapRegion.MapRegion Center( this MapRegion.MapRegion region, float latitude, float longitude )
    {
        region.CenterLatitude = latitude;
        region.CenterLongitude = longitude;

        return region;
    }

    public static MapRegion.MapRegion Offset( this MapRegion.MapRegion region, float xOffset, float yOffset )
    {
        region.CenterXOffset = xOffset;
        region.CenterYOffset = yOffset;

        return region;
    }

    public static MapRegion.MapRegion Scale( this MapRegion.MapRegion region, int value )
    {
        region.Scale = value;
        return region;
    }

    public static MapRegion.MapRegion RequestedHeight( this MapRegion.MapRegion region, float value )
    {
        region.RequestedHeight = value;
        return region;
    }

    public static MapRegion.MapRegion RequestedWidth( this MapRegion.MapRegion region, float value )
    {
        region.RequestedWidth = value;
        return region;
    }

    public static MapRegion.MapRegion Size( this MapRegion.MapRegion region, float height, float width )
    {
        region.RequestedHeight = height;
        region.RequestedWidth = width;

        return region;
    }

    public static MapRegion.MapRegion Heading( this MapRegion.MapRegion region, float value )
    {
        region.Heading = value;
        return region;
    }

    public static MapRegion.MapRegion MapStyle(this MapRegion.MapRegion region, string value)
    {
        region.MapStyle = value;
        return region;
    }
}
