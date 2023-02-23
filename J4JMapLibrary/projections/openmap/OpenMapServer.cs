﻿// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
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

namespace J4JSoftware.J4JMapLibrary;

public class OpenMapServer<TAuth> : MapServer<ITiledFragment, TAuth>, IOpenMapServer
where TAuth : class, IOpenMapCredentials, new()
{
    protected OpenMapServer()
    {
        ImageFileExtension = ".png";
        TileHeightWidth = 256;
    }

    public string RetrievalUrl { get; init; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;

#pragma warning disable CS1998
    public override async Task<bool> InitializeAsync( TAuth credentials, CancellationToken ctx = default )
#pragma warning restore CS1998
    {
        Initialized = false;

        UserAgent = credentials.UserAgent;
        Scale = MinScale;

        Initialized = true;
        return Initialized;
    }

    public override HttpRequestMessage? CreateMessage( ITiledFragment mapFragment, int scale )
    {
        if( !Initialized )
            return null;

        if( string.IsNullOrEmpty( UserAgent ) )
        {
            Logger.Error( "Undefined or empty User-Agent" );
            return null;
        }

        var replacements = new Dictionary<string, string>
        {
            { "{zoom}", scale.ToString() }, { "{x}", mapFragment.X.ToString() }, { "{y}", mapFragment.Y.ToString() }
        };

        var uriText = InternalExtensions.ReplaceParameters( RetrievalUrl, replacements );

        var retVal = new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );
        retVal.Headers.Add( "User-Agent", UserAgent );

        return retVal;
    }

    async Task<bool> IOpenMapServer.InitializeAsync( IOpenMapCredentials credentials, CancellationToken ctx )
    {
        if( credentials is TAuth castCredentials )
            return await InitializeAsync( castCredentials, ctx );

        Logger.Error( "Expected a {0} but got a {1} instead", typeof( TAuth ), credentials.GetType() );
        return false;
    }
}
