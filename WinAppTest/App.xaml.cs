#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// App.xaml.cs
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
using J4JSoftware.J4JMapWinLibrary;
using J4JSoftware.WindowsUtilities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace WinAppTest;

public partial class App : IWinApp
{
    public new static App Current => (App) Application.Current;

#pragma warning disable CS8618
    public App()
#pragma warning restore CS8618
    {
        InitializeComponent();

        var appInitializer = new WinAppInitializer( this );

        if( appInitializer.Initialize() )
        {
            LoggerFactory = Services?.GetService<ILoggerFactory>();
            MapControlViewModelLocator.Initialize( Services! );
        }
        else Exit();
    }

    public MainWindow? MainWindow { get; private set; }
    public IServiceProvider Services { get; set; }
    public IDataProtector AppConfigProtector { get; set; }
    public ILoggerFactory? LoggerFactory { get; }
    public bool SaveConfigurationOnExit { get; set; } = true;

    protected override void OnLaunched( LaunchActivatedEventArgs args )
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }
}
