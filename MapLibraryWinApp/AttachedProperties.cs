using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.MapLibrary;

public static class AttachedProperties
{
    #region TileCoordinates property

    public static readonly DependencyProperty CoordinatesProperty =
        DependencyProperty.RegisterAttached( nameof(CoordinatesProperty),
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
        nameof(IsMapTileProperty),
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
        nameof(IsFixedImageSizeProperty),
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
}