using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace J4JSoftware.MapLibrary
{
    public static class AttachedProperties
    {
        public static readonly DependencyProperty TileCoordinatesProperty =
            DependencyProperty.RegisterAttached( "TileCoordinatesProperty",
                                                 typeof( Coordinates ),
                                                 typeof( Image ),
                                                 null );

        public static Coordinates? GetTileCoordinates( Image image ) =>
            image.GetValue( TileCoordinatesProperty ) as Coordinates;

        public static void SetTileCoordinates( Image image, Coordinates value ) =>
            image.SetValue( TileCoordinatesProperty, value );

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
    }
}
