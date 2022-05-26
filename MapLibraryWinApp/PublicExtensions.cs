using System.Collections.Generic;
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
}