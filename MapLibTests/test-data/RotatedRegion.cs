#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// TileImage.cs
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

public class RotatedRegion : IEnumerable<object[]>
{
    public record ColumnRow( int AbsoluteColumn, int RegionColumn, int Row );

    public record Data(
        int CenterColumn,
        int CenterRow,
        int ColumnsWide,
        int RowsHigh,
        float Rotation,
        int Scale,
        ColumnRow[] Results
    );

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new Data( 0,
                      0,
                      2,
                      2,
                      0f,
                      2,
                      new[]
                      {
                          new ColumnRow( 3, -1, 0 ),
                          new ColumnRow( 0, 0, 0 ),
                          new ColumnRow( 1, 1, 0 ),
                          new ColumnRow( 3, -1, 1 ),
                          new ColumnRow( 0, 0, 1 ),
                          new ColumnRow( 1, 1, 1 )
                      } )
        };

        yield return new object[]
        {
            new Data( 0,
                      0,
                      2,
                      2,
                      45f,
                      2,
                      new[]
                      {
                          new ColumnRow( 3, -1, 0 ),
                          new ColumnRow( 0, 0, 0 ),
                          new ColumnRow( 1, 1, 0 ),
                          new ColumnRow( 3, -1, 1 ),
                          new ColumnRow( 0, 0, 1 ),
                          new ColumnRow( 1, 1, 1 )
                      } )
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
