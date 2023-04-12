#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MapTile.static.cs
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

using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary.MapRegion;

public partial class MapTile
{
    public static MapTile CreateStaticMapTile(
        IProjection projection,
        int xTile,
        int yTile,
        int scale,
        ILoggerFactory? loggerFactory
    )
    {
        var region = new MapRegion( projection, loggerFactory )
                    .Scale( scale )
                    .Size( projection.TileHeightWidth, projection.TileHeightWidth )
                    .Update();

        // determine the center point of the tile
        var upperLeftX = xTile * projection.TileHeightWidth;
        var upperLeftY = yTile * projection.TileHeightWidth;
        var centerPoint = new MapPoint( region );
        centerPoint.SetCartesian( upperLeftX + projection.TileHeightWidth / 2,
                                  upperLeftY + projection.TileHeightWidth / 2 );

        region.CenterLatitude = centerPoint.Latitude;
        region.CenterLongitude = centerPoint.Longitude;
        region.RequestedHeight = (float) projection.TileHeightWidth / 2;
        region.RequestedWidth = (float) projection.TileHeightWidth / 2;

        return new MapTile( region, yTile ).SetXAbsolute( 0 );
    }
}
