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

public abstract class OpenMapProjection : TiledProjection<string>
{
    private bool _authenticated;

    protected OpenMapProjection(
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( logger, tileCache )
    {
    }

    protected OpenMapProjection(
        IProjectionCredentials credentials,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( credentials, logger, tileCache )
    {
    }

    public override bool Initialized => base.Initialized && _authenticated;

    public override async Task<bool> AuthenticateAsync( string? credentials, CancellationToken ctx = default )
    {
        await base.AuthenticateAsync(credentials, ctx);

        if ( MapServer is not OpenMapServer mapServer )
        {
            Logger.Error( "Undefined or inaccessible IMessageCreator, cannot initialize" );
            return false;
        }

        credentials ??= LibraryConfiguration?.Credentials
                                             .Where( x => x.Name.Equals( Name, StringComparison.OrdinalIgnoreCase ) )
                                             .Select( x => x.ApiKey )
                                             .FirstOrDefault();

        if( credentials == null )
        {
            Logger.Error( "No credentials provided or available" );
            return false;
        }

        _authenticated = false;

        mapServer.UserAgent = credentials;
        MapScale.Scale = MapServer.MinScale;

        _authenticated = true;

        return true;
    }
}
