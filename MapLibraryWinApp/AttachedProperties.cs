using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace J4JSoftware.MapLibrary;

public static class AttachedProperties
{
    #region TileCoordinates property

    public static readonly DependencyProperty CoordinatesProperty =
        DependencyProperty.RegisterAttached( "CoordinatesProperty",
                                             typeof( Coordinates ),
                                             typeof( Image ),
                                             null );

    public static Coordinates? GetCoordinates( Image image ) =>
        image.GetValue( CoordinatesProperty ) as Coordinates;

    public static void SetCoordinates( Image image, Coordinates value ) =>
        image.SetValue( CoordinatesProperty, value );

    #endregion

    #region IsMapTile property

    public static readonly DependencyProperty IsMapTileProperty = DependencyProperty.RegisterAttached(
        "IsMapTileProperty",
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

}