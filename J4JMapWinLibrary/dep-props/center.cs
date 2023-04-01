using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public DependencyProperty CenterProperty = DependencyProperty.Register(nameof(Center),
                                                                           typeof(string),
                                                                           typeof(J4JMapControl),
                                                                           new PropertyMetadata(
                                                                               null,
                                                                               OnCenterChanged));

    private static void OnCenterChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if( d is not J4JMapControl mapControl )
            return;

        if( e.NewValue is not string centerText )
            return;

        if( !ConverterExtensions.TryParseToLatLong( centerText, out var latitude, out var longitude ) )
            mapControl._logger.Error( "Could not parse center '{0}' to latitude/longitude, defaulting to (0,0)",
                                      centerText );

        mapControl.MapRegion!.Center( latitude, longitude );
    }
}
