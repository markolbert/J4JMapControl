using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.J4JMapControl;

public static class PublicExtensions
{
    public static IEnumerable<Image> MapImages( this IEnumerable<UIElement> elements ) =>
        elements.OfType<Image>().Where( AttachedProperties.GetIsMapTile );

    public static IEnumerable<Image> VariableSizedMapImages( this IEnumerable<UIElement> elements ) =>
        elements.MapImages().Where( x => !AttachedProperties.GetIsFixedImageSize(x) );

    public static IEnumerable<Image> FixedSizedMapImages( this IEnumerable<UIElement> elements ) =>
        elements.MapImages().Where( AttachedProperties.GetIsFixedImageSize );

    public static IEnumerable<MultiCoordinates> MapCoordinates( this IEnumerable<Image> images ) =>
        images.Where( x => AttachedProperties.GetIsMapTile( x )
                       && AttachedProperties.GetCoordinates( x ) != null )
              .Select( x => AttachedProperties.GetCoordinates( x )! );

    public static void RemoveMapImages( this UIElementCollection elements, params MapTileState[] stateFilters )
    {
        var toRemove = elements.Select( ( x, i ) =>
                                {
                                    var image = x as Image;

                                    return new
                                    {
                                        Element = x,
                                        Index = i,
                                        IsMapTile = image != null && AttachedProperties.GetIsMapTile( image ),
                                        MapTileState = image == null
                                            ? MapTileState.NotAMapTile
                                            : AttachedProperties.GetMapTileState( image )
                                    };
                                } )
                               .Where( x => x.IsMapTile
                                        && ( stateFilters.Length == 0
                                            || stateFilters.Any( f => f == x.MapTileState ) ) )
                               .Select( x => x.Index )
                               .OrderByDescending( x => x );

        foreach( var idx in toRemove )
        {
            elements.RemoveAt( idx );
        }
    }

    public static void SetMapTileState( this UIElementCollection elements, MapTileState state )
    {
        foreach( var image in elements.MapImages() )
        {
            AttachedProperties.SetMapTileState( image, state );
        }
    }

    public static Image? GetMapTile(this UIElementCollection elements, MultiCoordinates toMatch) =>
        elements.MapImages()
                .FirstOrDefault( x => AttachedProperties.GetCoordinates( x ) == toMatch );
}