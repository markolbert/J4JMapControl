using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.J4JMapControl;

public static class Extensions
{
    public static IEnumerable<Image> MapImages( this IEnumerable<UIElement> elements ) =>
        elements.OfType<Image>().Where( AttachedProperties.GetIsMapTile );

    public static IEnumerable<Image> VariableSizedMapImages( this IEnumerable<UIElement> elements ) =>
        elements.MapImages().Where( x => AttachedProperties.GetCoordinates( x ) is LatLongCoordinates );

    public static IEnumerable<Image> FixedSizedMapImages( this IEnumerable<UIElement> elements ) =>
        elements.MapImages().Where( x => AttachedProperties.GetCoordinates( x ) is TileCoordinates);

    public static IEnumerable<Coordinates> MapCoordinates( this IEnumerable<Image> images ) =>
        images.Where( x => AttachedProperties.GetIsMapTile( x )
                       && AttachedProperties.GetCoordinates( x ) != null )
              .Select( x => AttachedProperties.GetCoordinates( x )! );
}