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

public class HorizontalRegion : IEnumerable<object[]>
{
    public record AbsoluteRelativeColumns( int AbsoluteColumn, int RegionColumn );

    public record Data(
        int CenterColumn,
        int ColumnsWide,
        int Scale,
        AbsoluteRelativeColumns[] Results
    );

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[]
        {
            new Data( 0,
                  3,
                  2,
                  new[]
                  {
                      new AbsoluteRelativeColumns( 3, -1 ),
                      new AbsoluteRelativeColumns( 0, 0 ),
                      new AbsoluteRelativeColumns( 1, 1 )
                  } )
        };

        yield return new object[]
        {
            new Data( 0,
                      5,
                      2,
                      new[]
                      {
                          new AbsoluteRelativeColumns( 2, -2 ),
                          new AbsoluteRelativeColumns( 3, -1 ),
                          new AbsoluteRelativeColumns( 0, 0 ),
                          new AbsoluteRelativeColumns( 1, 1 ),
                          new AbsoluteRelativeColumns( 2, 2 )
                      } )
        };

        yield return new object[]
        {
            new Data( 0,
                      8,
                      2,
                      new[]
                      {
                          new AbsoluteRelativeColumns( 2, -2 ),
                          new AbsoluteRelativeColumns( 3, -1 ),
                          new AbsoluteRelativeColumns( 0, 0 ),
                          new AbsoluteRelativeColumns( 1, 1 ),
                          new AbsoluteRelativeColumns( 2, 2 )
                      } )
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
