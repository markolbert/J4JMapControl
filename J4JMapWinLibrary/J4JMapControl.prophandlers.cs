using Microsoft.UI.Xaml;

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

        if (ConverterExtensions.TryParseToLatLong(centerText, out var latitude, out var longitude))
        {
            mapControl.CenterLatitude = latitude;
            mapControl.CenterLongitude = longitude;
        }
        else
        {
            mapControl._logger.Error("Could not parse Center ({0}), defaulting to 0/0", centerText);

            mapControl.CenterLatitude = 0;
            mapControl.CenterLongitude = 0;

            mapControl._ignoreChange = true;
            mapControl.Center = ConverterExtensions.ConvertToLatLongText( 0, 0 );
        }

        mapControl.UpdateFragments();
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

        if (e.NewValue is not string scaleText)
        {
            mapControl._logger.Error("OnMapScaleChanged did not receive a string value");
            return;
        }

        if (int.TryParse(scaleText, out var scale))
            mapControl.MapNumericScale = scale;
        else
        {
            mapControl._logger.Error("Could not parse MapScale ({0}), defaulting to 0", scaleText);

            mapControl.MapNumericScale = 0;

            mapControl._ignoreChange = true;
            mapControl.MapScale = "0";
        }

        mapControl.UpdateFragments();
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

        if (float.TryParse(headingText, out var heading))
            mapControl.NumericHeading = heading;
        else
        {
            mapControl._logger.Error("Could not parse Heading ({0}), defaulting to 0", headingText);

            mapControl.NumericHeading = 0;

            mapControl._ignoreChange = true;
            mapControl.Heading = "0";
        }

        mapControl.UpdateFragments();
    }
}
