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

    private static void OnCenterChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if (d is not J4JMapControl mapControl)
            return;

        if( mapControl._ignoreChange )
        {
            mapControl._ignoreChange = false;
            return;
        }

        if( e.NewValue is not string centerText )
        {
            mapControl._logger.Error( "OnCenterChanged did not receive a string value" );
            return;
        }

        if( mapControl.MapRegion == null )
        {
            mapControl._logger.Error("Projection not initialized, cannot set center");
            return;
        }

        if (ConverterExtensions.TryParseToLatLong(centerText, out var latitude, out var longitude))
        {
            mapControl.MapRegion.Center( latitude, longitude );
        }
        else
        {
            mapControl._logger.Error("Could not parse Center ({0}), defaulting to 0/0", centerText);

            mapControl.MapRegion.Center(0,0);

            mapControl._ignoreChange = true;
            mapControl.Center = ConverterExtensions.ConvertToLatLongText( 0, 0 );
        }

        mapControl.UpdateMapRegion();
    }

    private static void OnMapScaleChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if (d is not J4JMapControl mapControl)
            return;

        if (mapControl._ignoreChange)
        {
            mapControl._ignoreChange = false;
            return;
        }

        if (mapControl.MapRegion == null)
        {
            mapControl._logger.Error("Projection not initialized, cannot set scale");
            return;
        }

        mapControl.MapRegion.Scale( (int) mapControl.MapScale );

        mapControl.UpdateMapRegion();
    }

    private static void OnMinMaxScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not J4JMapControl mapControl)
            return;

        if (mapControl._ignoreChange)
        {
            mapControl._ignoreChange = false;
            return;
        }

        if( mapControl.MapScale >= mapControl.MinScale && mapControl.MapScale <= mapControl.MaxScale )
            return;

        mapControl.MapScale = mapControl.MapScale < mapControl.MinScale
            ? mapControl.MinScale
            : mapControl.MaxScale;
    }

    private static void OnHeadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not J4JMapControl mapControl)
            return;

        if (mapControl._ignoreChange)
        {
            mapControl._ignoreChange = false;
            return;
        }

        if (e.NewValue is not string headingText)
        {
            mapControl._logger.Error("OnHeadingChanged did not receive a string value");
            return;
        }

        if (mapControl.MapRegion == null)
        {
            mapControl._logger.Error("Projection not initialized, cannot set heading");
            return;
        }

        if( float.TryParse( headingText, out var heading ) )
            mapControl.MapRegion.Heading( heading );
        else
        {
            mapControl._logger.Error("Could not parse Heading ({0}), defaulting to 0", headingText);

            mapControl.MapRegion.Heading(0);

            mapControl._ignoreChange = true;
            mapControl.Heading = "0";
        }

        mapControl.UpdateMapRegion();
    }
}
