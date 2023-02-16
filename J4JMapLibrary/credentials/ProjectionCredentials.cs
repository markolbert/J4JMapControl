// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of ConsoleUtilities.
//
// ConsoleUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// ConsoleUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with ConsoleUtilities. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Extensions.Configuration;

namespace J4JMapLibrary;

public class ProjectionCredentials : IProjectionCredentials
{
    public ProjectionCredentials(
        IConfiguration config
    )
    {
        // we can't use the built-in IConfiguration stuff because
        // the objects we're creating are polymorphic (either a Credential
        // or a SignedCredential)
        var idx = 0;

        // thanx to Métoule for this!
        // https://stackoverflow.com/questions/55954858/how-to-load-polymorphic-objects-in-appsettings-json
        while( true )
        {
            var name = config.GetValue<string>( $"Credentials:{idx}:Name" );
            var apiKey = config.GetValue<string>( $"Credentials:{idx}:ApiKey" );
            var signature = config.GetValue<string>( $"Credentials:{idx}:SignatureSecret" );

            if( string.IsNullOrEmpty( name ) || string.IsNullOrEmpty( apiKey ) )
                break;

            var credential = string.IsNullOrEmpty( signature )
                ? new Credential { ApiKey = apiKey, Name = name }
                : new SignedCredential { ApiKey = apiKey, Name = name, Signature = signature };

            Credentials.Add( credential );

            idx++;
        }
    }

    public List<Credential> Credentials { get; } = new();

    public bool TryGetCredential( string projectionName, out Credential? result )
    {
        result = Credentials
           .FirstOrDefault( x => x.Name.Equals( projectionName, StringComparison.OrdinalIgnoreCase ) );

        return result != null;
    }
}
