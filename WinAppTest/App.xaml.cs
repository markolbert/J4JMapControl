// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using System;
using J4JSoftware.DeusEx;
using Serilog;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinAppTest;

public partial class App : Application
{
#pragma warning disable CS8618
    public App()
#pragma warning restore CS8618
    {
        this.InitializeComponent();

        var deusEx = new DeusEx();

        if( !deusEx.Initialize() )
        {
            J4JDeusEx.OutputFatalMessage("Could not initialize J4JDeusEx", null);
            throw new InvalidOperationException("Could not initialize J4JDeusEx");
        }

        Logger= J4JDeusEx.GetLogger<App>();
    }

    public ILogger? Logger { get; }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _mainWin = new MainWindow();
        _mainWin.Activate();
    }

    private Window _mainWin;
}
