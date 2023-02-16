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

namespace J4JSoftware.J4JMapLibrary;

public class OpenMapServer : MapServer<TiledFragment, string>
{
    protected OpenMapServer()
    {
        ImageFileExtension = ".png";
        TileHeightWidth = 256;
    }

    public string RetrievalUrl { get; init; } = string.Empty;
    public override bool Initialized => !string.IsNullOrEmpty( UserAgent );
    public string UserAgent { get; internal set; } = string.Empty;

    public override HttpRequestMessage? CreateMessage( TiledFragment mapFragment, int scale )
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
}
