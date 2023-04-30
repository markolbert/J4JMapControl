// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.System;
using J4JSoftware.DependencyInjection;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;
using Windows.Storage;
using J4JSoftware.J4JMapWinLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.UI.Xaml.Controls;

namespace WinAppTest;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow
{
    private readonly ILogger? _logger;
    private readonly PointOfInterest _sanCarlos;

    private readonly ObservableCollection<PointOfInterest> _ptsOfInterest;
    private bool _scIncluded;
    private readonly ObservableCollection<RoutePoint> _route1 = new();
    private readonly ObservableCollection<RoutePoint> _route2 = new();

    public MainWindow()
    {
        var config = J4JDeusEx.ServiceProvider.GetService<IConfiguration>();
        if (config == null)
        {
            _logger?.LogCritical("IConfiguration is not defined");
            throw new ApplicationException("IConfiguration is not defined");
        }

        var loggerFactory = J4JDeusEx.ServiceProvider.GetService<ILoggerFactory>();
        if( loggerFactory == null )
        {
            _logger?.LogCritical("ILoggerFactory is not defined");
            throw new ApplicationException("ILoggerFactory is not defined");
        }

        _logger = loggerFactory?.CreateLogger<MainWindow>();

        MapControlViewModelLocator.Initialize(config, loggerFactory);

        this.InitializeComponent();

        var hWnd = WindowNative.GetWindowHandle( this );
        var windowId = Win32Interop.GetWindowIdFromWindow( hWnd );
        var appWindow = AppWindow.GetFromWindowId( windowId );

        appWindow.Resize( new SizeInt32( 800, 1000 ) );

        //_j4jHost = J4JDeusEx.ServiceProvider.GetService<IJ4JHost>()
        // ?? throw new NullReferenceException( $"Could not load {typeof( IJ4JHost )}" );

        _ptsOfInterest = new ObservableCollection<PointOfInterest>
        {
            new PointOfInterest
            {
                Location = "37.5202N, 122.2758W",
                Text = "Belmont",
                Brush = new SolidColorBrush( Colors.BlanchedAlmond )
            },
            new PointOfInterest
            {
                Location = "37.5072N, 122.2605W",
                Text = "San Carlos",
                Brush = new SolidColorBrush( Colors.Gold )
            },
            new PointOfInterest
            {
                Location = "37.4848N, 122.2281W",
                Text = "Redwood City",
                Brush = new SolidColorBrush( Colors.Red )
            }
        };

        Task.Run( () => LoadRouteFileAsync( "route1.txt", _route1 ) ).Wait();
        Task.Run( () => LoadRouteFileAsync( "route2.txt", _route2 ) ).Wait();

        _sanCarlos = _ptsOfInterest[ 1 ];
        _scIncluded = true;

        SetFileSystemCachePath();
        UpdateStats();
    }

    private async Task LoadRouteFileAsync( string fileName, ObservableCollection<RoutePoint> routePoints )
    {
        routePoints.Clear();

        var uri = new Uri( $"ms-appx:///Assets/test-routes/{fileName}" );
        var file = await StorageFile.GetFileFromApplicationUriAsync( uri );
        var lines = await FileIO.ReadLinesAsync( file );

        for( var idx = 0; idx < lines.Count; idx++ )
        {
            var line = lines[ idx ];

            var parts = line.Split( ',' );
            if( parts.Length != 2 )
            {
                _logger?.LogWarning( "Invalid route entry {entry}", line );
                continue;
            }

            if( !float.TryParse( parts[ 1 ], out var latitude ) )
            {
                _logger?.LogWarning( "Invalid latitude {text}", parts[ 0 ] );
                continue;
            }

            if( !float.TryParse( parts[ 0 ], out var longitude ) )
            {
                _logger?.LogWarning( "Invalid longitude {text}", parts[ 0 ] );
                continue;
            }

            routePoints.Add( new RoutePoint( latitude.ToString( CultureInfo.CurrentCulture ),
                                             longitude.ToString( CultureInfo.CurrentCulture ),
                                             idx % 3 == 0 ) );
        }
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

    private void ChangeSanCarlosLabel( object sender, RoutedEventArgs e )
    {
        if( _sanCarlos.Text.Contains( "city", StringComparison.OrdinalIgnoreCase ) )
        {
            _sanCarlos.Text = "San Carlos";
            changeNameButton.Content = "Switch to City";
        }
        else
        {
            _sanCarlos.Text = "City of Good Living";
            changeNameButton.Content = "Switch to San Carlos";
        }
    }

    private void AddDeleteSanCarlosLabel(object sender, RoutedEventArgs e)
    {
        if( _scIncluded )
        {
            _ptsOfInterest.Remove( _sanCarlos );
            _scIncluded = false;
            addDeleteButton.Content = "Show San Carlos";
        }
        else
        {
            _ptsOfInterest.Add( _sanCarlos );
            _scIncluded = true;
            addDeleteButton.Content = "Hide San Carlos";
        }
    }

    private async void ShowCredentialsDialog( object sender, RoutedEventArgs e )
    {
        var bingDlg = new BingCredentialsDialog { XamlRoot = this.Content.XamlRoot };
        var result = await bingDlg.ShowAsync();
    }
}
