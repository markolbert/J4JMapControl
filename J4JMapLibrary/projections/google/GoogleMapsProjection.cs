// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
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

using J4JSoftware.Logging;

namespace J4JSoftware.J4JMapLibrary;

[ Projection( "GoogleMaps" ) ]
public sealed class GoogleMapsProjection : StaticProjection<GoogleCredentials>
{
    public GoogleMapsProjection(
        IJ4JLogger logger,
        IGoogleMapsServer? mapServer = null

    )
        : base( logger )
    {
        MapServer = mapServer ?? new GoogleMapsServer();
    }

    public GoogleMapsProjection(
        IProjectionCredentials credentials,
        IJ4JLogger logger,
        IGoogleMapsServer? mapServer = null
    )
        : base( credentials, logger )
    {
        MapServer = mapServer ?? new GoogleMapsServer();
    }

    public override async Task<bool> AuthenticateAsync(
        GoogleCredentials? credentials,
        CancellationToken ctx = default
    )
    {
        if( MapServer is not IGoogleMapsServer googleServer )
        {
            Logger.Error( "Undefined or inaccessible IMessageCreator, cannot initialize" );
            return false;
        }

        if( credentials != null )
            return await googleServer.InitializeAsync( credentials, ctx );

        if( LibraryConfiguration?
           .Credentials
           .FirstOrDefault( x => x.Name.Equals( Name, StringComparison.OrdinalIgnoreCase ) ) is not SignedCredential
           signedCredential )
        {
            Logger.Error( "Configuration credential not found or is not a SignedCredential" );
            return false;
        }

        credentials = new GoogleCredentials( signedCredential.ApiKey, signedCredential.Signature );

        return await googleServer.InitializeAsync( credentials, ctx );
    }
}
