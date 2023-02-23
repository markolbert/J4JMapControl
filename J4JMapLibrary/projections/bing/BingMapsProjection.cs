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

[ Projection( "BingMaps" ) ]
public sealed class BingMapsProjection : TiledProjection<BingCredentials>
{
    public const string MetadataUrl =
        "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/{mode}?output=json&key={apikey}";

    private bool _authenticated;

    public BingMapsProjection(
        IJ4JLogger logger,
        ITileCache? tileCache = null,
        IBingMapServer? mapServer = null
    )
        : base( logger )
    {
        MapServer = mapServer ?? new BingMapServer();
        TileCache = tileCache;
    }

    public override bool Initialized => base.Initialized && _authenticated;

    public override async Task<bool> AuthenticateAsync( BingCredentials credentials, CancellationToken ctx = default )
    {
        await base.AuthenticateAsync( credentials, ctx );

        if( MapServer is not IBingMapServer bingMapServer )
        {
            Logger.Error( "MapServer was not initialized with an instance of IBingMapServer" );
            return false;
        }

        _authenticated = await bingMapServer.InitializeAsync( credentials, ctx );

        return _authenticated;
    }
}
