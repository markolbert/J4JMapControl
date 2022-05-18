using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.J4JMapControl;

public static class PublicExtensions
{
    public static IEnumerable<Image> MapImages( this IEnumerable<UIElement> elements ) =>
        elements.OfType<Image>().Where( MapProperties.GetIsMapTile );

    public static IEnumerable<Image> VariableSizedMapImages( this IEnumerable<UIElement> elements ) =>
        elements.MapImages().Where( x => !MapProperties.GetIsFixedImageSize(x) );

    public static IEnumerable<Image> FixedSizedMapImages( this IEnumerable<UIElement> elements ) =>
        elements.MapImages().Where( MapProperties.GetIsFixedImageSize );

    public static IEnumerable<MultiCoordinates> MapCoordinates( this IEnumerable<Image> images ) =>
        images.Where( x => MapProperties.GetIsMapTile( x )
                       && MapProperties.GetCoordinates( x ) != null )
              .Select( x => MapProperties.GetCoordinates( x )! );

    public static void RemoveMapImages( this UIElementCollection elements, params MapTileState[] stateFilters )
    {
        UIElement? element;

        while( ( element = elements.FirstOrDefault( x =>
              {
                  var image = x as Image;

                  if( image == null || !MapProperties.GetIsMapTile( image ) )
                      return false;

                  var state = MapProperties.GetMapTileState( image );
                  return stateFilters.Contains( state );
              } ) ) != null )
        {
            elements.Remove( element );
        }
    }

    public static void SetMapTileState( this UIElementCollection elements, MapTileState state )
    {
        foreach( var image in elements.MapImages() )
        {
            MapProperties.SetMapTileState( image, state );
        }
    }

    public static Image? GetMapTile(this UIElementCollection elements, MultiCoordinates toMatch) =>
        elements.MapImages()
                .FirstOrDefault( x => MapProperties.GetCoordinates( x ) == toMatch );

    public static IEnumerable<AnnotationElement> GetInitializedAnnotations( this UIElementCollection uiElements,
        Size clipSize, IMapProjection mapProjection )
    {
        var sorted = uiElements.Select( x => new AnnotationElement( x ) )
            .Where( x => x.AnnotationInfo != null && x.AnnotationInfo.Initialize( clipSize, mapProjection ) )
            .OrderBy( x => x.AnnotationInfo!.Layer );

        foreach( var element in sorted )
        {
            yield return element;
        }
    }

    public static IEnumerable<AnnotationElement> GetValidAnnotations( this UIElementCollection uiElements )
    {
        var sorted = uiElements.Select( x => new AnnotationElement( x ) )
            .Where( x => x.AnnotationInfo != null && x.AnnotationInfo.IsValid)
            .OrderBy( x => x.AnnotationInfo!.Layer );

        foreach( var element in sorted )
        {
            yield return element;
        }
    }

    public static void ReplaceAnnotations( this UIElementCollection elements,
        IEnumerable<UIElement> annotations )
    {
        UIElement? element;

        while ((element = elements.FirstOrDefault(x =>
               {
                   return MapProperties.GetAnnotationProperty( x ) != null;
               })) != null)
        {
            elements.Remove(element);
        }

        foreach (var annotation in annotations.Where(x => x.GetValue(MapProperties.AnnotationProperty) != null))
        {
            elements.Add(annotation);
        }
    }
}