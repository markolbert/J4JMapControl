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

using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

[ Projection( "OpenTopoMaps" ) ]
public sealed class OpenTopoMapsProjection : TiledProjection<OpenTopoCredentials>
{
    public OpenTopoMapsProjection(
        ILoggerFactory? loggerFactory = null,
        ITileCache? tileCache = null
    )
        : base( null, loggerFactory )
    {
        TileCache = tileCache;
        ImageFileExtension = ".png";
        TileHeightWidth = 256;
        MinScale = 0;
        MaxScale = 15;
        MaxRequestLatency = 5000;
        RetrievalUrl = "https://tile.opentopomap.org/{zoom}/{x}/{y}.png";
        Copyright = "© OpenTopoMap-Mitwirkende, SRTM | Kartendarstellung\n© OpenTopoMap\nCC-BY-SA";
        CopyrightUri = new Uri( "http://opentopomap.org/" );
    }

    public string RetrievalUrl { get; }
    public string UserAgent { get; private set; } = string.Empty;

#pragma warning disable CS1998
    protected override async Task<bool> AuthenticateAsync( CancellationToken ctx = default )
#pragma warning restore CS1998
    {
        if( !await base.AuthenticateAsync( ctx ) )
            return false;

        Initialized = false;

        UserAgent = Credentials!.UserAgent;

        Initialized = true;
        return Initialized;
    }

    protected override HttpRequestMessage? CreateMessage( MapTile mapTile )
    {
        if( !Initialized )
        {
            Logger?.LogError("Projection not initialized");
            return null;
        }

        if( !mapTile.InProjection )
        {
            Logger?.LogError("MapTile not in the projection");
            return null;
        }

        if( string.IsNullOrEmpty( UserAgent ) )
        {
            Logger?.LogError("Undefined or empty User-Agent");
            return null;
        }

        var replacements = new Dictionary<string, string>
        {
            { "{zoom}", mapTile.Region.Scale.ToString() },
            { "{x}", mapTile.X.ToString() },
            { "{y}", mapTile.Y.ToString() }
        };

        var uriText = InternalExtensions.ReplaceParameters( RetrievalUrl, replacements );

        var retVal = new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );
        retVal.Headers.Add( "User-Agent", UserAgent );

        return retVal;
    }
}
