using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.MapLibrary;

public static class PublicExtensions
{
    public static List<MultiCoordinates> ExtractCoordinates( this IEnumerable<Image> images )
    {
        var retVal = new List<MultiCoordinates>();

        foreach( var image in images )
        {
            var coordinates = MapProperties.GetCoordinates( image );
            if( coordinates != null )
                retVal.Add( coordinates );
        }

        return retVal;
    }

    public static Point ToControlSpacePoint( this IMapProjection mapProjection, TilePoint tilePoint ) =>
        new Point( tilePoint.X * mapProjection.TileWidthHeight, tilePoint.Y * mapProjection.TileWidthHeight );

    // thanx to 3dGrabber for this
    // https://stackoverflow.com/questions/383587/how-do-you-do-integer-exponentiation-in-c
    public static int Pow(this int bas, int exp) =>
        Enumerable
           .Repeat(bas, Math.Abs(exp))
           .Aggregate(1, (a, b) => exp < 0 ? a / b : a * b);

    public static LatLong Capped(this LatLong toCheck, MapRetrieverInfo retrieverInfo)
    {
        var absLat = Math.Abs(toCheck.Latitude);
        if (absLat > retrieverInfo.MaximumLatitude)
            toCheck.Latitude = Math.Sign(toCheck.Latitude) * absLat;

        var absLng = Math.Abs(toCheck.Longitude);
        if (absLng > retrieverInfo.MaximumLongitude)
            toCheck.Longitude = Math.Sign(toCheck.Longitude) * absLng;

        return toCheck;
    }

    public static double DegreesToRadians(this double degrees) => degrees * Math.PI / 180;
    public static double RadiansToDegrees(this double radians) => radians * 180 / Math.PI;
}