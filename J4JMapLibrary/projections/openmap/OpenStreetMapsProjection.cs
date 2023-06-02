#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// OpenStreetMapsProjection.cs
//
// This file is part of JumpForJoy Software's J4JMapLibrary.
// 
// J4JMapLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapLibrary. If not, see <https://www.gnu.org/licenses/>.

#endregion

using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

[ Projection( "OpenStreetMaps" ) ]
public sealed class OpenStreetMapsProjection : TiledProjection
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

    protected override bool ValidateCredentials( object credentials )
    {
        if( credentials is OpenStreetCredentials )
            return true;

        Logger?.LogError( "Expected a {correct} but got a {incorrect} instead",
                          typeof( OpenStreetCredentials ),
                          credentials.GetType() );

        return false;
    }

    public override async Task<bool> AuthenticateAsync( CancellationToken ctx = default )
    {
        if( !await base.AuthenticateAsync( ctx ) )
            return false;

        Initialized = false;

        UserAgent = ( (OpenStreetCredentials) Credentials! ).UserAgent;

        // user agent must be unique, but this is all we can do to check it
        Initialized = !string.IsNullOrEmpty( UserAgent );

        if( !Initialized )
            Logger?.LogError( "Empty or undefined user agent value" );

        return Initialized;
    }

    protected override HttpRequestMessage? CreateMessage( MapBlock mapBlock )
    {
        if( !Initialized )
        {
            Logger?.LogError( "Projection not initialized" );
            return null;
        }

        if( mapBlock is not TileBlock castBlock )
        {
            Logger?.LogError( "Expected a {type} but got a {badType}", typeof( TileBlock ), mapBlock.GetType() );
            return null;
        }

        if( string.IsNullOrEmpty( UserAgent ) )
        {
            Logger?.LogError( "Undefined or empty User-Agent" );
            return null;
        }

        var replacements = new Dictionary<string, string>
        {
            { "{zoom}", castBlock.Scale.ToString() },
            { "{x}", castBlock.X.ToString() },
            { "{y}", castBlock.Y.ToString() }
        };

        var uriText = InternalExtensions.ReplaceParameters( RetrievalUrl, replacements );

        var retVal = new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );
        retVal.Headers.Add( "User-Agent", UserAgent );

        return retVal;
    }
}
