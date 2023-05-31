#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// CredentialsFactory.cs
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
using J4JSoftware.J4JMapLibrary;

namespace WinAppTest;

internal class CredentialsFactory : CredentialsFactoryBase
{
    private readonly AppConfiguration _config;

    public CredentialsFactory(
        AppConfiguration config
    )
    {
        _config = config;
    }

    protected override ICredentials? CreateCredentials( Type credType )
    {
        if( credType == typeof( BingCredentials ) )
            return _config.Credentials?.BingCredentials;

        if (credType == typeof(GoogleCredentials))
            return _config.Credentials?.GoogleCredentials;

        if (credType == typeof(OpenStreetCredentials))
            return _config.Credentials?.OpenStreetCredentials;

        return credType == typeof(OpenTopoCredentials) ? _config.Credentials?.OpenTopoCredentials : null;
    }
}
