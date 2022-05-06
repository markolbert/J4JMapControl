using System.Collections.Generic;
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.MapLibrary;

public static class PublicExtensions
{
    public static List<Coordinates> ExtractCoordinates( this IEnumerable<Image> images )
    {
        var retVal = new List<Coordinates>();

        foreach( var image in images )
        {
            var coordinates = AttachedProperties.GetCoordinates( image );
            if( coordinates != null )
                retVal.Add( coordinates );
        }

        return retVal;
    }

    public static MapRect GetPixelMapRect(this IZoom zoom, LatLong center, Size size)
    {
        var centerMapPt = new MapPoint(zoom);
        centerMapPt.LatLong.Set(center);

        var xOffset = size.Width / 2;
        var yOffset = size.Height / 2;

        var upperLeft = centerMapPt.OffsetByPixel(-xOffset, -yOffset);
        var lowerRight = centerMapPt.OffsetByPixel(xOffset, yOffset);

        var retVal = new MapRect(upperLeft, lowerRight);
        return retVal;
    }
}