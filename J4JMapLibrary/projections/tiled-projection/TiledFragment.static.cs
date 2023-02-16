// Copyright (c) 2021, 2022 Mark A. Olbert 
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

namespace J4JMapLibrary;

public partial class TiledFragment
{
    // ignoreCaches prevents loops when creating tiles from within the caching system
    public static async Task<TiledFragment> CreateAsync(
        ITiledProjection projection,
        int x,
        int y,
        int scale,
        bool ignoreCache = false,
        CancellationToken ctx = default
    )
    {
        if( projection.TileCache == null || ignoreCache )
            return new TiledFragment( projection, x, y, scale );

        var entry = await projection.TileCache.GetEntryAsync( projection, x, y, scale, ctx: ctx );
        return entry != null ? entry.Tile : new TiledFragment( projection, x, y, scale );
    }
}
