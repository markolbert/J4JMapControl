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

using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

public abstract class StaticProjection<TAuth> : Projection<TAuth>
    where TAuth : class, new()
{
    protected StaticProjection(
        ILoggerFactory? loggerFactory = null
    )
        : base( loggerFactory )
    {
    }

    // there's only ever one valid tile in a static projection (0,0)
    // so these next two methods are the same for static projections
    public override async Task<MapTile> GetMapTileByProjectionCoordinatesAsync(
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

    public override async Task<MapTile> GetMapTileByRegionCoordinatesAsync(
        int x,
        int y,
        int scale,
        CancellationToken ctx = default
    ) =>
        await GetMapTileByProjectionCoordinatesAsync( x, y, scale, ctx );

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
