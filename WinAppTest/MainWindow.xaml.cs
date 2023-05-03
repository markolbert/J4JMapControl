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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.System;
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
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;

namespace WinAppTest;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow
{
    private readonly IDataProtector _protector;
    private readonly ILogger? _logger;
    private readonly AppConfiguration _appConfiguration;
    private readonly JsonSerializerOptions _jsonOptions;

    private readonly PointOfInterest _sanCarlos;
    private readonly ObservableCollection<PointOfInterest> _ptsOfInterest;
    private readonly ObservableCollection<RoutePoint> _route1 = new();
    private readonly ObservableCollection<RoutePoint> _route2 = new();

    private bool _scIncluded;
    private bool _deleteOnSave;

    public MainWindow()
    {
        var config = GetRequiredService<IConfiguration>();
        var loggerFactory = GetRequiredService<ILoggerFactory>();
        _protector = GetRequiredService<IDataProtector>();
        _appConfiguration = GetRequiredService<AppConfiguration>();

        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        _logger = loggerFactory?.CreateLogger<MainWindow>();

        MapControlViewModelLocator.Initialize(config, loggerFactory);

        this.InitializeComponent();

        var hWnd = WindowNative.GetWindowHandle( this );
        var windowId = Win32Interop.GetWindowIdFromWindow( hWnd );
        var appWindow = AppWindow.GetFromWindowId( windowId );

        appWindow.Resize( new SizeInt32( 800, 1000 ) );

        this.Closed += ( _, _ ) => SaveConfiguration();

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

        mapControl.FileSystemCachePath = Path.Combine(AppConfiguration.UserFolder, "map-cache");
        UpdateStats();

        mapControl.NewCredentials += MapControlOnNewCredentials;

        if( _appConfiguration.UserConfigurationFileExists )
        {
            mapControl.MapProjection = _appConfiguration.MapProjection;
            mapControl.Center = _appConfiguration.Center ?? "0,0";
            mapControl.Heading = _appConfiguration.Heading;
            mapControl.MapScale = _appConfiguration.Scale;
        }
        else
        {
            mapControl.MapProjection = "BingMaps";
            mapControl.Center = "37.5072N,122.2605W";
            numberBoxHeading.Value = 0;
            numberBoxScale.Value = 16;
        }
    }

    private T GetRequiredService<T>()
        where T : class
    {
        //var retVal = J4JDeusEx.ServiceProvider.GetService<T>();
        var retVal = App.Current.Services.GetService<T>();
        if( retVal != null )
            return retVal;

        _logger?.LogCritical( "{service} is not available", typeof( T ) );
        throw new ApplicationException($"Service {typeof(T)} is not available");
    }

    private void MapControlOnNewCredentials( object? sender, NewCredentialsEventArgs e )
    {
        _appConfiguration.Credentials ??= new MapCredentials();
        _appConfiguration.MapProjection = e.ProjectionName;

        foreach ( var credProp in e.Credentials
                                   .CredentialProperties
                                   .Where(x=>x.Value?.ToString() != null) )
        {
            switch( e.ProjectionName.ToLower() )
            {
                case "bingmaps":
                    _appConfiguration.Credentials.BingCredentials ??= new BingCredentials();
                    _appConfiguration.Credentials.BingCredentials.ApiKey = (string) credProp.Value!;

                    break;

                case "googlemaps":
                    _appConfiguration.Credentials.GoogleCredentials ??= new GoogleCredentials();

                    switch ( credProp.PropertyName )
                    {
                        case nameof(GoogleCredentials.ApiKey):
                            _appConfiguration.Credentials.GoogleCredentials.ApiKey = (string)credProp.Value!;
                            break;

                        case nameof(GoogleCredentials.SignatureSecret):
                            _appConfiguration.Credentials.GoogleCredentials.ApiKey = (string)credProp.Value!;
                            break;
                    }

                    break;

                case "openstreetmaps":
                    _appConfiguration.Credentials.OpenStreetCredentials ??= new OpenStreetCredentials();
                    _appConfiguration.Credentials.OpenStreetCredentials.UserAgent = (string) credProp.Value!;
                    break;

                case "opentopomaps":
                    _appConfiguration.Credentials.OpenTopoCredentials ??= new OpenTopoCredentials();
                    _appConfiguration.Credentials.OpenTopoCredentials.UserAgent = (string)credProp.Value!;
                    break;
            }
        }
    }

    private void SaveConfiguration()
    {
        if (string.IsNullOrEmpty(_appConfiguration.UserConfigurationFilePath))
            return;

        if( _deleteOnSave )
        {
            File.Delete(_appConfiguration.UserConfigurationFilePath);
            return;
        }

        _appConfiguration.Center = mapControl.Center;
        _appConfiguration.MapProjection = mapControl.MapProjection;
        _appConfiguration.Scale = Convert.ToInt32( mapControl.MapScale );
        _appConfiguration.Heading = mapControl.Heading;

        var encrypted = _appConfiguration.Encrypt( _protector );

        try
        {
            var jsonText = JsonSerializer.Serialize( encrypted, _jsonOptions );

            File.WriteAllText( _appConfiguration.UserConfigurationFilePath!, jsonText );
        }
        catch( Exception ex )
        {
            _logger?.LogError( "Failed to write configuration file, exception was '{exception}'", ex.Message );
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
        await bingDlg.ShowAsync();
    }

    private void DeleteOnSave_OnClick( object sender, RoutedEventArgs e )
    {
        _deleteOnSave = deleteOnSaveChk.IsChecked ?? false;
    }
}
