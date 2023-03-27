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
using Serilog;

namespace J4JSoftware.J4JMapLibrary;

public abstract class StaticProjection<TAuth> : Projection<TAuth>
    where TAuth : class, new()
{
    protected StaticProjection(
        ILogger logger
    )
        : base( logger )
    {
    }

    // xIsRelative is ignored because there's only ever one valid
    // tile in a static projection (0,0)
    public override async Task<MapTile> GetMapTileAsync(
        int x,
        int y,
        int scale,
        bool xIsRelative = false,
        CancellationToken ctx = default
    )
    {
        var retVal = MapTile.CreateStaticMapTile( this, x, y, scale, Logger );
        await retVal.LoadImageAsync( ctx );

        return retVal;
    }

    protected override async Task<bool> LoadRegionInternalAsync( MapRegion.MapRegion region, CancellationToken ctx = default )
    {
        if( region.IsDefined)
            return await region.MapTiles[0,0].LoadImageAsync( ctx );

        Logger.Error("Undefined static MapRegion");
        return false;
    }
}
