#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MainWindow.xaml.cs
//
// This file is part of JumpForJoy Software's WinAppTest.
// 
// WinAppTest is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// WinAppTest is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with WinAppTest. If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapWinLibrary;
using J4JSoftware.WindowsUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace WinAppTest;

public sealed partial class MainWindow
{
    private readonly AppConfiguration _appConfig;
    private readonly ILogger? _logger;

    private readonly PointOfInterest _sanCarlos;
    private readonly ObservableCollection<PointOfInterest> _ptsOfInterest;
    private readonly ObservableCollection<RoutePoint> _route1 = new();
    private readonly ObservableCollection<RoutePoint> _route2 = new();

    private bool _scIncluded;

#pragma warning disable CS8618
    public MainWindow()
#pragma warning restore CS8618
    {
        InitializeComponent();

        _logger = App.Current.LoggerFactory?.CreateLogger<MainWindow>();

        var winSerializer = new MainWinSerializer( this );

        var temp = App.Current.Services.GetService<AppConfiguration>();
        if( temp == null )
        {
            _logger?.LogCritical( "Could not retrieve instance of {type}", typeof( AppConfiguration ) );
            App.Current.Exit();
            return;
        }

        _appConfig = temp;

        winSerializer.SetSizeAndPosition();

        _ptsOfInterest = new ObservableCollection<PointOfInterest>
        {
            new()
            {
                Location = "37.5202N, 122.2758W",
                Text = "Belmont",
                Brush = new SolidColorBrush( Colors.BlanchedAlmond )
            },
            new()
            {
                Location = "37.5072N, 122.2605W",
                Text = "San Carlos",
                Brush = new SolidColorBrush( Colors.Gold )
            },
            new()
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

        mapControl.FileSystemCachePath = Path.Combine( WinUIConfigBase.UserFolder, "map-cache" );
        UpdateStats();

        mapControl.ValidCredentials += MapControlOnNewCredentials;

        if( _appConfig.UserConfigurationFileExists )
        {
            mapControl.MapProjection = _appConfig.MapProjection;
            mapControl.Center = _appConfig.Center ?? "0,0";
            mapControl.Heading = _appConfig.Heading;
            mapControl.MapScale = _appConfig.Scale;
        }
        else
        {
            mapControl.MapProjection = "BingMaps";
            mapControl.Center = "37.5072N,122.2605W";
            numberBoxHeading.Value = 0;
            numberBoxScale.Value = 16;
        }
    }

    private void MapControlOnNewCredentials( object? sender, ValidCredentialsEventArgs e )
    {
        _appConfig.Credentials ??= new MapCredentials();
        _appConfig.MapProjection = e.ProjectionName;

        switch( e.Credentials )
        {
            case BingCredentials bingCredentials:
                _appConfig.Credentials.BingCredentials = bingCredentials;
                break;

            case GoogleCredentials googleCredentials:
                _appConfig.Credentials.GoogleCredentials = googleCredentials;
                break;

            case OpenStreetCredentials streetCredentials:
                _appConfig.Credentials.OpenStreetCredentials = streetCredentials;
                break;

            case OpenTopoCredentials topoCredentials:
                _appConfig.Credentials.OpenTopoCredentials = topoCredentials;
                break;

            default:
                _logger?.LogError( "Unsupported credentials type '{type}'", e.Credentials.GetType() );
                break;
        }
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

        if( string.IsNullOrEmpty( headingText.Text ) )
            return;

        e.Handled = true;

        mapControl.SetHeadingByText( headingText.Text );
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

    private void AddDeleteSanCarlosLabel( object sender, RoutedEventArgs e )
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
        var bingDlg = new BingCredentialsDialog { XamlRoot = Content.XamlRoot };
        await bingDlg.ShowAsync();
    }

    private void DeleteOnSave_OnClick( object sender, RoutedEventArgs e )
    {
        App.Current.SaveConfigurationOnExit = deleteOnSaveChk.IsChecked ?? false;
    }
}
