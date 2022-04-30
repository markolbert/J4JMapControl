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
            DependencyProperty.RegisterAttached( "TileCoordinates",
                                                 typeof( Coordinates ),
                                                 typeof( Image ),
                                                 null );

        public static Coordinates GetTileCoordinates( Image bitmapSource ) =>
            (Coordinates) bitmapSource.GetValue( TileCoordinatesProperty );

        public static void SetTileCoordinates( Image bitmapSource, Coordinates value ) =>
            bitmapSource.SetValue( TileCoordinatesProperty, value );
    }
}
