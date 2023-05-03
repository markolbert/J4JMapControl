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

using Microsoft.UI.Xaml;
using System;
using System.IO;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapWinLibrary;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text.Json;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Microsoft.Extensions.Hosting;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace WinAppTest;

public partial class App
{
#pragma warning disable CS8618
    public new static App Current => (App)Application.Current;

    private Window _mainWin;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly IDataProtector _dataProtector;
    private AppConfiguration? _appConfig;
    
    public App()
#pragma warning restore CS8618
    {
        this.InitializeComponent();

        var logFile = Path.Combine(AppConfiguration.UserFolder, "log.txt");

        var seriLogger = new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .WriteTo.Debug()
                        .WriteTo.File(logFile, rollingInterval: RollingInterval.Hour)
                        .CreateLogger();

        _loggerFactory = new LoggerFactory().AddSerilog(seriLogger);
        _logger = _loggerFactory.CreateLogger<App>();

        var localAppFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var dpProvider = DataProtectionProvider.Create(
            new DirectoryInfo( Path.Combine( localAppFolder, "ASP.NET", "DataProtection-Keys" ) ) );
        _dataProtector = dpProvider.CreateProtector(nameof(WinAppTest));

        var configPath = Path.Combine(AppConfiguration.UserFolder, "userConfig.json");

        try
        {
            Services = new HostBuilder()
                      .ConfigureHostConfiguration( builder => ParseUserConfigFile( configPath, builder ) )
                      .ConfigureServices( ( hbc, s ) => ConfigureServices( hbc, s ) )
                      .Build()
                      .Services;
        }
        catch( CryptographicException crypto )
        {
            var logger = _loggerFactory.CreateLogger<App>();
            logger.LogError( "Cryptographic error '{mesg}', deleting user configuration file '{path}'",
                             crypto.Message,
                             configPath );

            File.Delete( configPath );

            Exit();
        }
        catch( JsonException )
        {
            Exit();
        }

        MapControlViewModelLocator.Initialize( Services! );
    }

    public IServiceProvider Services { get; }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private IServiceCollection ConfigureServices( HostBuilderContext hbc, IServiceCollection services )
    {
        services.AddSingleton( _loggerFactory );
        services.AddSingleton( _dataProtector );

        services.AddSingleton( new ProjectionFactory( _loggerFactory ) );
        services.AddSingleton( new CredentialsFactory( hbc.Configuration, _loggerFactory ) );
        services.AddSingleton( new CredentialsDialogFactory( _loggerFactory ) );
        services.AddSingleton( _appConfig! );

        return services;
    }

    private void ParseUserConfigFile( string path, IConfigurationBuilder builder )
    {
        var fileExists = File.Exists( path );
        if( !fileExists )
        {
            _logger.LogWarning( "Could not find user config file '{path}', creating default configuration", path );
            _appConfig = new AppConfiguration() { UserConfigurationFilePath = path };

            return;
        }

        var encrypted = JsonSerializer.Deserialize<AppConfiguration>( File.ReadAllText( path ) );

        if( encrypted == null )
        {
            _logger.LogError( "Could not parse user config file '{path}'", path );
            throw new JsonException( $"Could not parse user config file '{path}'" );
        }

        encrypted.UserConfigurationFilePath = path;
        encrypted.UserConfigurationFileExists = File.Exists( path );

        _appConfig = encrypted.Decrypt( _dataProtector );
        _appConfig.AddToConfiguration( builder );
        _appConfig.UserConfigurationFilePath = path;
        _appConfig.UserConfigurationFileExists = true;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWin = new MainWindow();
        _mainWin.Activate();
    }
}
