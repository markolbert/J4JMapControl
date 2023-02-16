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

namespace J4JSoftware.J4JMapLibrary;

public partial class TiledFragment : MapFragment
{
    // internal to hopefully avoid stack overflow
    internal TiledFragment(
        ITiledProjection projection,
        int xTile,
        int yTile,
        int scale,
        byte[] imageData
    )
        : this( projection, xTile, yTile, scale )
    {
        ImageData = imageData;
    }

    private TiledFragment(
        ITiledProjection projection,
        int xTile,
        int yTile,
        int scale
    )
        : base( projection )
    {
        TiledScale = projection.TiledScale;
        HeightWidth = projection.MapServer.TileHeightWidth;

        X = xTile < 0 ? 0 : xTile;
        Y = yTile < 0 ? 0 : yTile;
        QuadKey = this.GetQuadKey( scale );
    }
}
