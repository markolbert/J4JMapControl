// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using Windows.Graphics;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;

namespace WinAppTest;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow
{
    public MainWindow()
    {
        this.InitializeComponent();

        var hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        appWindow.Resize( new SizeInt32( 800, 800 ) );
    }
}
