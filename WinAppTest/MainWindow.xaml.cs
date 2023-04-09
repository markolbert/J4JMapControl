// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Graphics;
using J4JSoftware.DependencyInjection;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

        SetFileSystemCachePath();
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
    }

    private void ClearCache( object sender, RoutedEventArgs e )
    {
        mapControl.ClearCache();
    }

    private void StatsClick( object sender, RoutedEventArgs e )
    {
        CacheStats.Clear();

        foreach( var item in mapControl.GetCacheStats() )
        {
            CacheStats.Add( item );
        }
    }

    private ObservableCollection<CacheStats> CacheStats { get; } = new();
}
