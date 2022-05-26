using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.MapLibrary;

public class MapProperties : DependencyObject
{
    #region TileCoordinates property

    public static readonly DependencyProperty CoordinatesProperty =
        DependencyProperty.RegisterAttached( "Coordinates",
                                             typeof( MultiCoordinates ),
                                             typeof( Image ),
                                             null );

    public static MultiCoordinates? GetCoordinates( Image image ) =>
        image.GetValue( CoordinatesProperty ) as MultiCoordinates;

    public static void SetCoordinates( Image image, MultiCoordinates value ) =>
        image.SetValue( CoordinatesProperty, value );

    #endregion

    #region IsMapTile property

    public static readonly DependencyProperty IsMapTileProperty = DependencyProperty.RegisterAttached(
        "IsMapTile",
        typeof( bool ),
        typeof( Image ),
        null );

    public static bool GetIsMapTile( Image image )
    {
        var propValue = image.GetValue( IsMapTileProperty );

        return propValue != null && (bool) propValue;
    }

    public static void SetIsMapTile( Image image, bool value ) => image.SetValue( IsMapTileProperty, value );

    #endregion

    #region IsFixedImageSize property

    public static readonly DependencyProperty IsFixedImageSizeProperty = DependencyProperty.RegisterAttached(
        "IsFixedImageSize",
        typeof(bool),
        typeof(Image),
        null);

    public static bool GetIsFixedImageSize(Image image)
    {
        var propValue = image.GetValue(IsFixedImageSizeProperty);

        return propValue != null && (bool)propValue;
    }

    public static void SetIsFixedImageSize( Image image, bool value ) =>
        image.SetValue( IsFixedImageSizeProperty, value );

    #endregion

    #region MapTileState property

    public static readonly DependencyProperty MapTileStateProperty = DependencyProperty.RegisterAttached(
        "MapTileState",
        typeof(MapTileState),
        typeof(Image),
        null);

    public static MapTileState GetMapTileState(Image image)
    {
        var isMapTile = MapProperties.GetIsMapTile( image );

        var propValue = image.GetValue(MapTileStateProperty);

        return propValue == null 
            ? isMapTile 
                ? MapTileState.NotSet 
                : MapTileState.NotAMapTile 
            : (MapTileState) propValue;
    }

    public static void SetMapTileState(Image image, MapTileState value) =>
        image.SetValue(MapTileStateProperty, value);

    #endregion

    #region Annotation property

    // the layer to which a map annotation belongs
    public static readonly DependencyProperty AnnotationProperty = DependencyProperty.RegisterAttached(
        "Annotation",
        typeof( AnnotationBase ),
        typeof( UIElement ),
        new PropertyMetadata( -1 )
    );

    public static AnnotationBase? GetAnnotationProperty( UIElement element ) =>
        element.GetValue( AnnotationProperty ) as AnnotationBase;

    public static void SetAnnotationProperty( UIElement element, AnnotationBase annotation ) =>
        element.SetValue( AnnotationProperty, annotation );

    #endregion
}