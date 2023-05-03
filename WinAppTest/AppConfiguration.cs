#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// AppConfiguration.cs
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using J4JSoftware.J4JMapLibrary;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;

namespace WinAppTest;

public class AppConfiguration
{
    public static string UserFolder { get; } = Windows.Storage.ApplicationData.Current.LocalFolder.Path;

    [JsonIgnore]
    public bool UserConfigurationFileExists { get; set; }

    [JsonIgnore]
    public string? UserConfigurationFilePath { get; set; }
    
    public MapCredentials? Credentials { get; set; }
    public string? MapProjection { get; set; }
    public string? Center { get; set; }
    public int Scale { get; set; }
    public double Heading { get; set; }

    public void AddToConfiguration( IConfigurationBuilder builder )
    {
        var keyValuePairs = new List<KeyValuePair<string, string?>>
        {
            new( nameof( UserConfigurationFileExists ),
                 UserConfigurationFileExists.ToString() ),
            new( nameof( UserConfigurationFilePath ),
                 UserConfigurationFilePath ),
            new( nameof( MapProjection ), MapProjection ),
            new( nameof( Center ), Center ),
            new( nameof( Scale ), Scale.ToString() ),
            new( nameof( Heading ), Heading.ToString( CultureInfo.CurrentCulture ) )
        };

        if( Credentials is { GoogleCredentials: not null } )
            keyValuePairs.AddRange( Credentials.GoogleCredentials.ToKeyValuePairs() );

        if( Credentials is { BingCredentials: not null } )
            keyValuePairs.AddRange( Credentials.BingCredentials.ToKeyValuePairs() );

        if( Credentials is { OpenStreetCredentials: not null } )
            keyValuePairs.AddRange( Credentials.OpenStreetCredentials.ToKeyValuePairs() );

        if( Credentials is { OpenTopoCredentials: not null } )
            keyValuePairs.AddRange( Credentials.OpenTopoCredentials.ToKeyValuePairs() );

        builder.AddInMemoryCollection( keyValuePairs );
    }

    public AppConfiguration Encrypt( IDataProtector protector )
    {
        var retVal = new AppConfiguration
        {
            UserConfigurationFilePath = UserConfigurationFilePath,
            UserConfigurationFileExists = UserConfigurationFileExists,
            MapProjection = MapProjection,
            Center = Center,
            Scale = Scale,
            Heading = Heading,
            Credentials = new MapCredentials()
        };

        if( Credentials?.GoogleCredentials != null )
            retVal.Credentials.GoogleCredentials =
                (GoogleCredentials) Credentials.GoogleCredentials.Encrypt( protector );

        if( Credentials?.BingCredentials != null )
            retVal.Credentials.BingCredentials = (BingCredentials) Credentials.BingCredentials.Encrypt( protector );

        if( Credentials?.OpenStreetCredentials != null )
            retVal.Credentials.OpenStreetCredentials =
                (OpenStreetCredentials) Credentials.OpenStreetCredentials.Encrypt( protector );

        if( Credentials?.OpenTopoCredentials != null )
            retVal.Credentials.OpenTopoCredentials =
                (OpenTopoCredentials) Credentials.OpenTopoCredentials.Encrypt( protector );

        return retVal;
    }

    public AppConfiguration Decrypt(IDataProtector protector)
    {
        var retVal = new AppConfiguration
        {
            UserConfigurationFilePath = UserConfigurationFilePath,
            UserConfigurationFileExists = UserConfigurationFileExists,
            MapProjection = MapProjection,
            Center = Center,
            Scale = Scale,
            Heading = Heading,
            Credentials = new MapCredentials()
        };

        if (Credentials?.GoogleCredentials != null)
            retVal.Credentials.GoogleCredentials =
                (GoogleCredentials)Credentials.GoogleCredentials.Decrypt(protector);

        if (Credentials?.BingCredentials != null)
            retVal.Credentials.BingCredentials = (BingCredentials)Credentials.BingCredentials.Decrypt(protector);

        if (Credentials?.OpenStreetCredentials != null)
            retVal.Credentials.OpenStreetCredentials =
                (OpenStreetCredentials)Credentials.OpenStreetCredentials.Decrypt(protector);

        if (Credentials?.OpenTopoCredentials != null)
            retVal.Credentials.OpenTopoCredentials =
                (OpenTopoCredentials)Credentials.OpenTopoCredentials.Decrypt(protector);

        return retVal;
    }
}