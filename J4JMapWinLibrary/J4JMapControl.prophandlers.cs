using System.Numerics;
using Microsoft.UI.Xaml;
using J4JSoftware.J4JMapLibrary;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private static void OnCachingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not J4JMapControl mapControl)
            return;

        mapControl._cacheIsValid = false;
        mapControl.UpdateCaching();
    }

    private static void OnUpdateIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not J4JMapControl mapControl)
            return;

        if( e.NewValue is not int value )
            return;

        if( value < 0 )
        {
            mapControl._logger.Warning( "Tried to set UpdateEventInterval < 0, defaulting to {0}",
                                        J4JMapControl.DefaultUpdateEventInterval );
            value = DefaultUpdateEventInterval;
        }

        mapControl.UpdateEventInterval = value;
    }

    private static void OnMapProjectionChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if (d is not J4JMapControl mapControl)
            return;

        mapControl.UpdateProjection();
    }

    private static void OnMinMaxScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not J4JMapControl mapControl)
            return;

        if( mapControl.MapScale >= mapControl.MinScale && mapControl.MapScale <= mapControl.MaxScale )
            return;

        mapControl.MapScale = mapControl.MapScale < mapControl.MinScale
            ? mapControl.MinScale
            : mapControl.MaxScale;
    }

    private static void OnCenterChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if (d is not J4JMapControl mapControl)
            return;

        if (e.NewValue is not string centerText)
            return;

        if (!ConverterExtensions.TryParseToLatLong(centerText, out var latitude, out var longitude))
            mapControl._logger.Error("Could not parse center '{0}' to latitude/longitude, defaulting to (0,0)",
                                      centerText);

        mapControl.MapRegion!.Center(latitude, longitude);
    }

    private static void OnMapScaleChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if (d is not J4JMapControl mapControl)
            return;

        if (e.NewValue is not double mapScale)
            return;

        mapControl.MapRegion!.Scale((int)mapScale);
    }

    private static void OnHeadingChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if (d is not J4JMapControl mapControl)
            return;

        if (e.NewValue is not double heading)
            return;

        mapControl.MapRegion!.Heading((float)heading);
    }
}
