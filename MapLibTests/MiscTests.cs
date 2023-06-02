#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MiscTests.cs
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

using FluentAssertions;
using J4JSoftware.J4JMapLibrary;

namespace MapLibTests;

public class MiscTests
{
    [ Theory ]
    [ InlineData( "0", 1, 0, 0 ) ]
    [ InlineData( "01", 2, 1, 0 ) ]
    [ InlineData( "032", 3, 2, 3 ) ]
    public void DecodeQuadKeys( string quadKey, int scale, int x, int y )
    {
        MapExtensions.TryParseQuadKey( quadKey, out var deconstructed )
                     .Should()
                     .BeTrue();

        deconstructed.Should().NotBeNull();

        deconstructed!.Scale.Should().Be( scale );
        deconstructed.XTile.Should().Be( x );
        deconstructed.YTile.Should().Be( y );
    }
}
