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

    public override async Task<MapTile> GetMapTileAsync( int x, int y, int scale, CancellationToken ctx = default )
    {
        //var region = new MapRegion( this, Logger ) { Scale = scale };

        //// determine the center point of the tile
        //var upperLeftX = x * TileHeightWidth;
        //var upperLeftY = y * TileHeightWidth;
        //var centerPoint = new StaticPoint( this ) { Scale = scale };
        //centerPoint.SetCartesian( upperLeftX + TileHeightWidth / 2, upperLeftY + TileHeightWidth / 2 );

        //region.CenterLatitude = centerPoint.Latitude;
        //region.CenterLongitude = centerPoint.Longitude;
        //region.RequestedHeight = (float) TileHeightWidth / 2;
        //region.RequestedWidth = (float) TileHeightWidth / 2;

        //var retVal = new MapTile( region, x, y );

        var retVal = MapTile.CreateMapTile( this, x, y, scale, Logger );
        
        await retVal!.LoadImageAsync( ctx );
    
        return retVal;
    }

    protected override async Task<bool> LoadRegionInternalAsync( MapRegion.MapRegion region, CancellationToken ctx = default ) =>
        await region.MapTiles.First().LoadImageAsync( ctx );
}
