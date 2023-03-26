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

    private static void OnMapProjectionChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if( d is not J4JMapControl mapControl )
            return;

        mapControl.UpdateProjection();
    }

    private static void OnMapConfigurationChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if( d is not J4JMapControl mapControl )
            return;

        mapControl.MapConfigurationChanged();
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
}
