﻿// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
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

public class TiledFragment : MapFragment, ITiledFragment
{
    public TiledFragment(
        ITiledProjection projection,
        int xTile,
        int yTile,
        int scale
    )
        : base( projection )
    {
        Scale = projection.ScaleRange.ConformValueToRange( scale, "QuadKey Scale" );
        HeightWidth = projection.TileHeightWidth;
        ImageHeight = HeightWidth;
        ImageWidth = HeightWidth;

        XTile = xTile < 0 ? 0 : xTile;
        YTile = yTile < 0 ? 0 : yTile;
        QuadKey = this.GetQuadKey( Scale );
        FragmentId = QuadKey;
    }

    public int HeightWidth { get; }

    public string QuadKey { get; }
}