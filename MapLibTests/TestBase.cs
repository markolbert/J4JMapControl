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
using J4JSoftware.DeusEx;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MapLibTests;

public class TestBase
{
    public static readonly string[] ProjectionNames =
        new[] { "BingMaps", "OpenStreetMaps", "OpenTopoMaps", "GoogleMaps" };

    protected TestBase()
    {
        var deusEx = new DeusEx();
        deusEx.Initialize().Should().BeTrue();

        LoggerFactory = J4JDeusEx.ServiceProvider.GetRequiredService<ILoggerFactory>();
        LoggerFactory.Should().NotBeNull();
        Logger = LoggerFactory.CreateLogger( GetType() );

        ProjectionFactory = GetRequiredService<ProjectionFactory>();
        CredentialsFactory = GetRequiredService<CredentialsFactory>();
    }

    private T GetRequiredService<T>()
        where T : class
    {
        var service = J4JDeusEx.ServiceProvider.GetRequiredService<T>();
        service.Should().NotBeNull();

        return service;
    }

    protected ILoggerFactory LoggerFactory { get; }
    protected ILogger Logger { get; }

    protected ProjectionFactory ProjectionFactory { get; }
    protected CredentialsFactory CredentialsFactory { get; }

    protected IProjection? CreateAndAuthenticateProjection( string projName )
    {
        var retVal = ProjectionFactory.CreateProjection(projName);
        if ( retVal == null )
            return retVal;

        var credentials = CredentialsFactory[projName];
        if( credentials == null )
            return retVal;

        retVal!.SetCredentials(credentials!);
        retVal.Authenticate();

        return retVal;
    }

    protected static string GetCheckImagesFolder(string projectionName)
    {
        var retVal = Environment.CurrentDirectory;

        for (var idx = 0; idx < 3; idx++)
        {
            retVal = Path.GetDirectoryName(retVal)!;
        }

        retVal = Path.Combine(retVal, "check-images", projectionName);
        Directory.CreateDirectory(retVal);

        return retVal;
    }

}
