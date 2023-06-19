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

using J4JSoftware.VisualUtilities;
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
        var retVal = new StaticBlock( this, x, y, scale );
        await LoadImageAsync( retVal, ctx );

        return retVal;
    }

    public override async Task<IMapRegion?> LoadRegionAsync(
        Region region,
        CancellationToken ctx = default( CancellationToken )
    )
    {
        var area = region.Area;
        if( area == null )
            return null;

        var heightWidth = GetHeightWidth( region.Scale );

        var projRectangle = new Rectangle2D( heightWidth, heightWidth, coordinateSystem: CoordinateSystem2D.Display );
        var shrinkResult = projRectangle.ShrinkToFit( area, region.ShrinkStyle );

        var retVal = new StaticMapRegion { Zoom = shrinkResult.Zoom, Block = new StaticBlock( this, region ) };

        retVal.ImagesLoaded = await LoadImageAsync( retVal.Block, ctx );
        OnRegionProcessed( retVal.ImagesLoaded );

        return retVal;
    }

    protected override async Task<bool> LoadBlocksInternalAsync(
        IEnumerable<MapBlock> blocks,
        CancellationToken ctx = default
    )
    {
        var mapBlock = blocks.FirstOrDefault();

        if( mapBlock != null )
            return await LoadImageAsync( mapBlock, ctx );

        Logger?.LogError( "Undefined static MapRegion" );
        return false;
    }
}
