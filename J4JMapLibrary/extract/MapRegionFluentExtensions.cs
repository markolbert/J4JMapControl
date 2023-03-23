using J4JSoftware.J4JMapLibrary.MapRegion;
using System.Text;

namespace J4JSoftware.J4JMapLibrary;

public static class MapRegionFluentExtensions
{
    public static string GetQuadKey(this MapTile mapTile)
    {
        // static projections only have a single quadkey, defaulting to "0"
        if (mapTile.Region.Projection is not ITiledProjection)
            return "0";

        var retVal = new StringBuilder();

        for (var i = mapTile.Region.Scale; i > mapTile.Region.Projection.ScaleRange.Minimum - 1; i--)
        {
            var digit = '0';
            var mask = 1 << (i - 1);

            if ((mapTile.RetrievedX & mask) != 0)
                digit++;

            if ((mapTile.RetrievedY & mask) != 0)
            {
                digit++;
                digit++;
            }

            retVal.Append(digit);
        }

        return retVal.ToString();
    }

    public static MapRegion.MapRegion CenterLatitude( this MapRegion.MapRegion region, float value )
    {
        region.CenterLatitude = value;
        return region;
    }

    public static MapRegion.MapRegion CenterLongitude(this MapRegion.MapRegion region, float value)
    {
        region.CenterLongitude = value;
        return region;
    }

    public static MapRegion.MapRegion Center(this MapRegion.MapRegion region, float latitude, float longitude)
    {
        region.CenterLatitude = latitude;
        region.CenterLongitude = longitude;

        return region;
    }

    public static MapRegion.MapRegion Scale(this MapRegion.MapRegion region, int value)
    {
        region.Scale= value;
        return region;
    }
    public static MapRegion.MapRegion RequestedHeight(this MapRegion.MapRegion region, float value)
    {
        region.RequestedHeight = value;
        return region;
    }
    public static MapRegion.MapRegion RequestedWidth(this MapRegion.MapRegion region, float value)
    {
        region.RequestedWidth = value;
        return region;
    }

    public static MapRegion.MapRegion Size(this MapRegion.MapRegion region, float height, float width)
    {
        region.RequestedHeight = height;
        region.RequestedWidth = width;

        return region;
    }

    public static MapRegion.MapRegion Heading(this MapRegion.MapRegion region, float value)
    {
        region.Heading = value;
        return region;
    }
}
