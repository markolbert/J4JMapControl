// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Windows.Graphics;
using Windows.System;
using Windows.UI;
using J4JSoftware.DependencyInjection;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;

namespace WinAppTest;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow
{
    private readonly ILogger? _logger;

    public MainWindow()
    {
        this.InitializeComponent();

        var loggerFactory = ( (App) Application.Current ).LoggerFactory;
        _logger = loggerFactory?.CreateLogger<MainWindow>();
        mapControl.LoggerFactory = loggerFactory;

        mapControl.ProjectionFactory = J4JDeusEx.ServiceProvider.GetService<ProjectionFactory>();
        if( mapControl.ProjectionFactory == null )
            _logger?.LogCritical( "ProjectionFactory is not defined" );

        var hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        appWindow.Resize( new SizeInt32( 800, 1000 ) );

        mapControl.PointsOfInterestSource = new List<PointOfInterest>
        {
            new PointOfInterest( "37.5202N, 122.2758W", "Belmont", new SolidColorBrush( Colors.BlanchedAlmond ) ),
            new PointOfInterest( "37.5072N, 122.2605W", "San Carlos", new SolidColorBrush( Colors.Gold ) ),
            new PointOfInterest( "37.4848N, 122.2281W", "Redwood City", new SolidColorBrush( Colors.Red ) )
        };

        mapControl.PointsOfInterestLocationProperty = nameof( PointOfInterest.Location );

        SetFileSystemCachePath();
        UpdateStats();
    }

    private void TextHeadingLostFocus( object sender, RoutedEventArgs e )
    {
        if( string.IsNullOrEmpty( headingText.Text ) )
            return;

        mapControl.SetHeadingByText( headingText.Text );
        headingText.Text = string.Empty;
    }

    private void SetFileSystemCachePath()
    {
        var hostConfig = J4JDeusEx.ServiceProvider.GetService<IJ4JHost>();

        if( hostConfig == null )
            _logger?.LogError("Could not retrieve instance of IJ4JHost");
        else mapControl.FileSystemCachePath = Path.Combine( hostConfig.ApplicationConfigurationFolder, "map-cache" );
    }

    private void PurgeCache( object sender, RoutedEventArgs e )
    {
        mapControl.PurgeCache();
        UpdateStats();
    }

    private void ClearCache( object sender, RoutedEventArgs e )
    {
        mapControl.ClearCache();
        UpdateStats();
    }

    private void StatsClick( object sender, RoutedEventArgs e ) => UpdateStats();

    private void UpdateStats()
    {
        CacheStats.Clear();

        foreach( var stats in mapControl.GetCacheStats() )
        {
            CacheStats.Add( stats );
        }

        // this is ugly code...but as it's for testing...
        cacheGrid.ItemsSource = null;
        cacheGrid.ItemsSource = CacheStats;
    }

    private ObservableCollection<CacheStats> CacheStats { get; } = new();

    private void TextHeadingKeyUp( object sender, KeyRoutedEventArgs e )
    {
        if( e.Key != VirtualKey.Enter )
            return;

        if (string.IsNullOrEmpty(headingText.Text))
            return;

        e.Handled = true;

        mapControl.SetHeadingByText(headingText.Text);
        headingText.Text = string.Empty;
    }
}
