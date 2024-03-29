﻿#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// StaticProjection.cs
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

public abstract class StaticProjection : Projection
{
    protected StaticProjection(
        IEnumerable<string>? mapStyles = null,
        ILoggerFactory? loggerFactory = null
    )
        : base( mapStyles, loggerFactory )
    {
    }

    // there's only ever one valid tile in a static projection (0,0)
    // so these next two methods are the same for static projections
    public override async Task<MapTile> GetMapTileWraparoundAsync(
        int x,
        int y,
        int scale,
        CancellationToken ctx = default
    )
    {
        var retVal = MapTile.CreateStaticMapTile( this, x, y, scale, LoggerFactory );
        await LoadImageAsync( retVal, ctx );

        return retVal;
    }

    public override async Task<MapTile> GetMapTileAbsoluteAsync(
        int x,
        int y,
        int scale,
        CancellationToken ctx = default
    ) =>
        await GetMapTileWraparoundAsync( x, y, scale, ctx );

    protected override async Task<bool> LoadRegionInternalAsync(
        MapRegion.MapRegion region,
        CancellationToken ctx = default
    )
    {
        if( region.IsDefined )
            return await LoadImageAsync( region.MapTiles[ 0, 0 ], ctx );

        Logger?.LogError( "Undefined static MapRegion" );
        return false;
    }
}
