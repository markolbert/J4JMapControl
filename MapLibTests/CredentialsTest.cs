#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// CredentialsTest.cs
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

public class CredentialsTest : TestBase
{
    [ Fact ]
    public void Bing()
    {
        var credentials = CredentialsFactory[ "BingMaps" ] as BingCredentials;
        credentials.Should().NotBeNull();

        credentials!.ApiKey.Should().NotBeEmpty();
    }

    [ Fact ]
    public void Google()
    {
        var credentials = CredentialsFactory[ "GoogleMaps" ] as GoogleCredentials;
        credentials.Should().NotBeNull();

        credentials!.ApiKey.Should().NotBeEmpty();
        credentials!.SignatureSecret.Should().NotBeEmpty();
    }

    [ Fact ]
    public void OpenStreetMaps()
    {
        var credentials = CredentialsFactory[ "OpenStreetMaps" ] as OpenStreetCredentials;
        credentials.Should().NotBeNull();

        credentials!.UserAgent.Should().NotBeEmpty();
    }

    [ Fact ]
    public void OpenTopoMaps()
    {
        var credentials = CredentialsFactory[ "OpenTopoMaps" ] as OpenTopoCredentials;
        credentials.Should().NotBeNull();

        credentials!.UserAgent.Should().NotBeEmpty();
    }
}
