#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// FactoryTests.cs
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

namespace MapLibTests;

public class FactoryTests : TestBase
{
    [ Theory ]
    [InlineData("BingMaps", true, true)]
    [InlineData("OpenStreetMaps", true, true)]
    [InlineData("OpenTopoMaps", true, true)]
    [InlineData("GoogleMaps", true, true)]
    [InlineData("UnknownMaps", false, false)]
    public void CreateProjectionFromName( string projectionName, bool projCreated, bool authenticated )
    {
        var projection = ProjectionFactory.CreateProjection( projectionName );

        if( !projCreated )
        {
            projection.Should().BeNull();
            return;
        }

        projection.Should().NotBeNull();

        var credentials = CredentialsFactory[projectionName];
        credentials.Should().NotBeNull();

        projection!.SetCredentials( credentials! ).Should().Be( authenticated );

        if( authenticated )
            projection.Authenticate().Should().BeTrue();
    }

    [Theory]
    [InlineData("BingMaps", true, true)]
    [InlineData("OpenStreetMaps", true, true)]
    [InlineData("OpenTopoMaps", true, true)]
    [InlineData("GoogleMaps", true, true)]
    [InlineData("UnknownMaps", false, false)]
    public async Task CreateProjectionFromNameAsync(string projectionName, bool projCreated, bool authenticated )
    {
        var projection = ProjectionFactory.CreateProjection(projectionName);

        if (!projCreated)
        {
            projection.Should().BeNull();
            return;
        }

        projection.Should().NotBeNull();

        var credentials = CredentialsFactory[projectionName];
        credentials.Should().NotBeNull();

        projection!.SetCredentials(credentials!).Should().Be(authenticated);

        if( !authenticated )
            return;

        var authResult = await projection.AuthenticateAsync();
        authResult.Should().Be( authenticated );
    }

    [Theory]
    [InlineData(typeof(BingMapsProjection), true, true)]
    [InlineData(typeof(OpenStreetMapsProjection), true, true)]
    [InlineData(typeof(OpenTopoMapsProjection), true, true)]
    [InlineData(typeof(GoogleMapsProjection), true, true)]
    [InlineData(typeof(string), false, false)]
    public void CreateProjectionFromType(Type projType, bool projCreated, bool authenticated)
    {
        var projection = ProjectionFactory.CreateProjection(projType);

        if (!projCreated)
        {
            projection.Should().BeNull();
            return;
        }

        projection.Should().NotBeNull();

        var credentials = CredentialsFactory[projType];
        credentials.Should().NotBeNull();

        projection!.SetCredentials(credentials!).Should().Be(authenticated);

        if (authenticated)
            projection.Authenticate().Should().BeTrue();
    }

    [Theory]
    [InlineData(typeof(BingMapsProjection), true, true)]
    [InlineData(typeof(OpenStreetMapsProjection), true, true)]
    [InlineData(typeof(OpenTopoMapsProjection), true, true)]
    [InlineData(typeof(GoogleMapsProjection), true, true)]
    [InlineData(typeof(string), false, false)]
    public async Task CreateProjectionFromTypeAsync(Type projType, bool projCreated, bool authenticated)
    {
        var projection = ProjectionFactory.CreateProjection(projType);

        if (!projCreated)
        {
            projection.Should().BeNull();
            return;
        }

        projection.Should().NotBeNull();

        var credentials = CredentialsFactory[projType];
        credentials.Should().NotBeNull();

        projection!.SetCredentials(credentials!).Should().Be(authenticated);

        if (!authenticated)
            return;

        var authResult = await projection.AuthenticateAsync();
        authResult.Should().Be(authenticated);
    }

    [Theory]
    [InlineData("BingMaps",".jpg")]
    [InlineData("OpenStreetMaps", ".png")]
    [InlineData("OpenTopoMaps", ".png")]
    [InlineData("GoogleMaps", ".png")]
    public void CheckImageFileExtension(string projectionName, string fileExtension)
    {
        var projection = CreateAndAuthenticateProjection(projectionName);
        projection.Should().NotBeNull();
        projection!.ImageFileExtension.Should().BeEquivalentTo( fileExtension );
    }
}
