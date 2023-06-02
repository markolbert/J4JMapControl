#region copyright
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

    public override async Task<MapBlock?> GetMapTileAsync(
        int x,
        int y,
        int scale,
        CancellationToken ctx = default
    )
    {
        var retVal = StaticBlock.CreateBlock( this, x, y, scale );
        if( retVal != null )
            await LoadImageAsync( retVal, ctx );

        return retVal;
    }

    protected override async Task<bool> LoadRegionInternalAsync(
        MapRegion.MapRegion region,
        CancellationToken ctx = default
    )
    {
        if( region.IsDefined )
        {
            var mapBlock = region.MapBlocks.FirstOrDefault()?.MapBlock;

            if( mapBlock != null )
                return await LoadImageAsync( mapBlock, ctx );
        }

        Logger?.LogError( "Undefined static MapRegion" );
        return false;
    }
}
