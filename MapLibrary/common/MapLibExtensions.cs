using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace J4JSoftware.MapLibrary;

public static class MapLibExtensions
{
    public static double GetGroundResolution( this IZoom zoom, LatLong globeCoord ) =>
        Math.Cos( globeCoord.Latitude * Math.PI / 180 )
      * 2
      * Math.PI
      * GlobalConstants.EarthRadius
      / zoom.WidthHeight;

    public static DoublePoint GetScreenCoordinates(this IZoom zoom, LatLong globeCoord)
    {
        var sinLatitude = Math.Sin(globeCoord.Latitude * Math.PI / 180);

        var x = ((globeCoord.Longitude + 180) / 360) * 256 * Math.Pow(2, zoom.Level);

        var y = (0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI))
          * 256
          * Math.Pow(2, zoom.Level);

        return new DoublePoint(x, y);
    }

    public static string GetBingMapsQuadKey( this MultiTileCoordinates multiTile )
    {
        var retVal = new StringBuilder();

        for (var i = multiTile.Zoom.Level; i > 0; i--)
        {
            var digit = '0';
            var mask = 1 << (i - 1);

            if ((multiTile.TileCoordinates.X & mask) != 0)
                digit++;

            if ((multiTile.TileCoordinates.Y & mask) != 0)
            {
                digit++;
                digit++;
            }

            retVal.Append(digit);
        }

        return retVal.ToString();
    }

    public static string GetDescription<TEnum>(this TEnum enumValue)
    {
        if( !typeof( TEnum ).IsEnum )
            throw new ArgumentException(
                $"Trying to retrieve Enum {nameof( DescriptionAttribute )} from a non-Enum type" );

        var enumText = enumValue?.ToString();

        if( string.IsNullOrEmpty(enumText))
            return "*** undefined ***";

        var enumMember = typeof( TEnum ).GetMember( enumText )[ 0 ];
        var svcAttr = enumMember.GetCustomAttribute<DescriptionAttribute>(false);

        return svcAttr?.Description ?? enumText;
    }

}