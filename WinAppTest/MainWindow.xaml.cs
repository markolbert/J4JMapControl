// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using System.IO;
using Windows.Graphics;
using J4JSoftware.DependencyInjection;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Serilog;
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

        _logger = ( (App) Application.Current ).Logger?.ForContext<MainWindow>();
        mapControl.Logger = _logger;

        mapControl.ProjectionFactory = J4JDeusEx.ServiceProvider.GetService<ProjectionFactory>();
        if( mapControl.ProjectionFactory == null )
            _logger?.Fatal( "ProjectionFactory is not defined" );

        var hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        appWindow.Resize( new SizeInt32( 800, 800 ) );

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
            _logger?.Error("Could not retrieve instance of IJ4JHost");
        else mapControl.FileSystemCachePath = Path.Combine( hostConfig.ApplicationConfigurationFolder, "map-cache" );
    }
}
