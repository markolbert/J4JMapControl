#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Tile.cs
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

using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary.MapRegion;

public class Tile
{
    public Tile(
        MapRegion region,
        int x,
        int y
    )
    {
        Region = region;
        Logger = region.LoggerFactory?.CreateLogger<MapTile>();

        ( X, Y ) = region.ProjectionType switch
        {
            ProjectionType.Static => ( 0, 0 ),
            ProjectionType.Tiled => ( x, y ),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( ProjectionType )} value '{region.ProjectionType}'" )
        };
    }

    protected ILogger? Logger { get; }

    public MapRegion Region { get; }

    // For Tile objects, X can be any value in the extended tile plane.
    // For MapTile objects, X can only take on values within the range of
    // minimum and maximum tile values for the scale of the MapRegion in
    // which the MapTile is created, or -1 if outside the MapRegion's 
    // horizontal limits, giving allowance for wrapping around the left
    // and right edges.

    // Wrapping is limited to including at most all the tiles
    // in the current horizontal range -- wrapping beyond that limits implies
    // the horizontal tile coordinate is outside the MapRegion.
    public int X { get; protected set; }
    public int Y { get; }

    public (int X, int Y) GetUpperLeftCartesian() =>
        Region.ProjectionType switch
        {
            ProjectionType.Static => ( 0, 0 ),
            ProjectionType.Tiled => ( X * Region.Projection.TileHeightWidth, Y * Region.Projection.TileHeightWidth ),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {typeof( ProjectionType )} value '{Region.ProjectionType}'" )
        };
}
