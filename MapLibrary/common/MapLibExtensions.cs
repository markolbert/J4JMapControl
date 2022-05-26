using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace J4JSoftware.MapLibrary;

public static class MapLibExtensions
{
    public static double GetGroundResolution( this IMapProjection mapProjection, LatLong latLong ) =>
        Math.Cos( latLong.Latitude * Math.PI / 180 )
      * 2
      * Math.PI
      * GlobalConstants.EarthRadius
      / mapProjection.ProjectionWidthHeight;

    public static string GetBingMapsQuadKey( this MultiCoordinates tile, int zoomLevel ) =>
        GetBingMapsQuadKey( tile.TilePoint.X, tile.TilePoint.Y, zoomLevel );

    public static string GetBingMapsQuadKey( int xTile, int yTile, int zoomLevel )
    {
        var retVal = new StringBuilder();

        for( var i = zoomLevel; i > 0; i-- )
        {
            var digit = '0';
            var mask = 1 << ( i - 1 );

            if( ( xTile & mask ) != 0 )
                digit++;

            if( ( yTile & mask ) != 0 )
            {
                digit++;
                digit++;
            }

            retVal.Append( digit );
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