using System.Collections.Generic;
using System.Numerics;
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

    public static DoublePoint ToDoublePoint( this Size size, IMapProjection mapProjection ) =>
        new( size.Width, size.Height, CoordinateOrigin.UpperLeft, mapProjection );

    public static DoublePoint ToDoublePoint( this Vector2 size, IMapProjection mapProjection ) =>
        new( size.X, size.Y, CoordinateOrigin.UpperLeft, mapProjection );
}