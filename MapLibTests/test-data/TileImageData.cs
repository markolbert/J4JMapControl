#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// TileImageData.cs
//
// This file is part of JumpForJoy Software's MapLibTests.
// 
// MapLibTests is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// MapLibTests is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with MapLibTests. If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections;

namespace MapLibTests;

public class TileImageData : IEnumerable<object[]>
{
    public record Tile( int Scale, int TileX, int TileY );

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { new Tile( 1, 0, 0 ) };
        yield return new object[] { new Tile( 2, 0, 0 ) };
        yield return new object[] { new Tile( 5, 27, 27 ) };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
