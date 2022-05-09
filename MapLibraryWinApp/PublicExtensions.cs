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
}