using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.MapLibrary;

public static class PublicExtensions
{
    public static List<MapTile> ExtractMapTiles( this IEnumerable<Image> images )
    {
        var retVal = new List<MapTile>();

        foreach( var image in images )
        {
            var tile = MapProperties.GetTile( image );
            if( tile != null )
                retVal.Add( tile );
        }

        return retVal;
    }

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

    public static Point Center( this Rect rect ) =>
        new( ( rect.Left + rect.Right ) / 2, ( rect.Top + rect.Bottom ) / 2 );
}