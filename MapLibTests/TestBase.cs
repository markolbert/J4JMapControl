#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// TestBase.cs
//
// This file is part of JumpForJoy Software's MapLibTests.
// 
// MapLibTests is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// MapLibTests is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with MapLibTests. If not, see <https://www.gnu.org/licenses/>.

#endregion

using FluentAssertions;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MapLibTests;

public class TestBase
{
    public static readonly string[] ProjectionNames = { "BingMaps", "OpenStreetMaps", "OpenTopoMaps", "GoogleMaps" };

    protected TestBase()
    {
        var logFile = Path.Combine( Environment.CurrentDirectory, "log.txt" );

        var seriLogger = new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .WriteTo.Debug()
                        .WriteTo.File( logFile, rollingInterval: RollingInterval.Hour )
                        .CreateLogger();

        LoggerFactory = new LoggerFactory().AddSerilog( seriLogger );
        LoggerFactory.Should().NotBeNull();
        Logger = LoggerFactory.CreateLogger( GetType() );

        Host = CreateHost();

        ProjectionFactory = Host.Services.GetRequiredService<ProjectionFactory>();
        CredentialsFactory = Host.Services.GetRequiredService<ICredentialsFactory>();
    }

    protected IHost Host { get; }

    private IHost CreateHost()
    {
        var builder = new HostBuilder();

        builder.ConfigureHostConfiguration( ConfigureHost );
        builder.ConfigureServices( ConfigureServices );

        return builder.Build();
    }

    private void ConfigureHost( IConfigurationBuilder builder )
    {
        builder.AddUserSecrets<TestBase>();
    }

    private void ConfigureServices( HostBuilderContext hbc, IServiceCollection services )
    {
        services.AddSingleton( _ =>
        {
            var retVal = new ProjectionFactory( LoggerFactory );
            retVal.InitializeFactory();

            return retVal;
        } );

        services.AddSingleton<ICredentialsFactory, CredentialsFactory>( _ =>
        {
            var credentials = hbc.Configuration.Get<MapCredentials>();
            credentials.Should().NotBeNull();
            credentials!.BingCredentials.Should().NotBeNull();
            credentials.GoogleCredentials.Should().NotBeNull();
            credentials.OpenStreetCredentials.Should().NotBeNull();
            credentials.OpenTopoCredentials.Should().NotBeNull();

            var retVal = new CredentialsFactory( credentials );
            retVal.InitializeFactory();

            return retVal;
        } );

        services.AddSingleton( new MemoryCache( "In Memory", LoggerFactory ) );
        services.AddSingleton( new FileSystemCache( "File System", LoggerFactory ) );
    }

    protected ILoggerFactory LoggerFactory { get; }
    protected ILogger Logger { get; }

    protected ProjectionFactory ProjectionFactory { get; }
    protected ICredentialsFactory CredentialsFactory { get; }

    protected IProjection? CreateAndAuthenticateProjection( string projName )
    {
        var retVal = ProjectionFactory.CreateProjection( projName );
        if( retVal == null )
            return retVal;

        var credentials = CredentialsFactory[ projName ];
        if( credentials == null )
            return retVal;

        retVal.SetCredentials( credentials );
        retVal.Authenticate();

        return retVal;
    }

    protected static string GetCheckImagesFolder( string projectionName )
    {
        var retVal = Environment.CurrentDirectory;

        for( var idx = 0; idx < 3; idx++ )
        {
            retVal = Path.GetDirectoryName( retVal )!;
        }

        retVal = Path.Combine( retVal, "check-images", projectionName );
        Directory.CreateDirectory( retVal );

        return retVal;
    }
}
