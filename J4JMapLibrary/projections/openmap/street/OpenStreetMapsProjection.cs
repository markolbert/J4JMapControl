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

[ Projection( "OpenStreetMaps" ) ]
public sealed class OpenStreetMapsProjection : TiledProjection<OpenStreetCredentials>
{
    public OpenStreetMapsProjection(
        ILoggerFactory? loggerFactory = null
    )
        : base( null, loggerFactory )
    {
        ImageFileExtension = ".png";
        TileHeightWidth = 256;
        MinScale = 0;
        MaxScale = 20;
        RetrievalUrl = "https://tile.openstreetmap.org/{zoom}/{x}/{y}.png";
        Copyright = "© OpenStreetMap Contributors";
        CopyrightUri = new Uri( "http://www.openstreetmap.org/copyright" );
    }

    public string RetrievalUrl { get; }
    public string UserAgent { get; private set; } = string.Empty;

    protected override async Task<bool> AuthenticateAsync( CancellationToken ctx = default )
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
            Logger?.LogError( "Projection not initialized" );
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
