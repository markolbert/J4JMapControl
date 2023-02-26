using System;
using System.Threading.Tasks;
using ABI.Windows.ApplicationModel.UserDataTasks;
using CommunityToolkit.WinUI;
using J4JSoftware.J4JMapLibrary;
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

    private static void OnViewportChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
        if( d is not J4JMapControl mapControl )
            return;

        if( mapControl._fragments == null )
        {
            mapControl._logger.Information( "Updating viewport before IProjection assigned" );
            return;
        }

        mapControl.UpdateNeeded = true;

        var viewport = mapControl.GetViewport();
        mapControl.IsValid = viewport != null;

        if( mapControl.IsValid )
            mapControl.ViewportChanged?.Invoke( mapControl,
                                                new ControlViewport( mapControl.Center,
                                                                     mapControl.MapScale,
                                                                     mapControl.Heading,
                                                                     viewport ) );
    }

}
