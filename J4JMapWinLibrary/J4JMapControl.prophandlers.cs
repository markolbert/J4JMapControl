using J4JSoftware.J4JMapLibrary;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private static void OnMapProjectionChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if( d is not J4JMapControl mapControl )
            return;

        if( e.NewValue is not IProjection projection )
        {
            mapControl._logger.Error( "Object assigned to MapProjection is not an IProjection" );
            return;
        }

        mapControl._fragments = new ImageFragments( projection, mapControl._logger );
        mapControl.InvalidateMeasure();
    }

    private static void OnViewportChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if( d is not J4JMapControl mapControl )
            return;

        if( mapControl._fragments == null )
        {
            mapControl._logger.Information( "Updating viewport before IProjection assigned" );
            return;
        }

        mapControl._fragments.Scale = mapControl.MapScale;
        mapControl._fragments.SetCenter( mapControl.Latitude, mapControl.Longitude );
        mapControl._fragments.Heading = mapControl.Heading;

        mapControl.InvalidateMeasure();
    }
}
