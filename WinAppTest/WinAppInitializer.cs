#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// WinAppInitializer.cs
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

using System.IO;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapWinLibrary;
using J4JSoftware.WindowsUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace WinAppTest;

internal class WinAppInitializer : WinAppInitializerBase<AppConfiguration>
{
    public WinAppInitializer(
        IWinApp winApp
    )
        : base( winApp )
    {
    }

    protected override LoggerConfiguration GetSerilogConfiguration()
    {
        var logFile = Path.Combine( WinUIConfigBase.UserFolder, "log.txt" );

        return new LoggerConfiguration()
              .MinimumLevel.Verbose()
              .WriteTo.Debug()
              .WriteTo.File( logFile, rollingInterval: RollingInterval.Hour );
    }

    protected override IServiceCollection ConfigureServices( HostBuilderContext hbc, IServiceCollection services )
    {
        base.ConfigureServices( hbc, services );

        services.AddSingleton( new ProjectionFactory( LoggerFactory ) );

        services.AddSingleton<ICredentialsFactory, CredentialsFactory>( s => new CredentialsFactory( AppConfig! ) );

        services.AddSingleton( new CredentialsDialogFactory( LoggerFactory ) );

        return services;
    }
}
