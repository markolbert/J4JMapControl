#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// StaticImage.cs
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
using J4JSoftware.J4JMapLibrary;

namespace MapLibTests;

public class StaticImage : IEnumerable<object[]>
{
    public record Region( float Latitude, float Longitude, int Scale, float Height, float Width )
    {
        public string FragmentId =>
            $"{MapExtensions.LatitudeToText( Latitude )}-{MapExtensions.LongitudeToText( Longitude )}-{Scale}-{Height}-{Width}";
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { new Region( 0, 0, 0, 256, 256 ) };
        yield return new object[] { new Region( 0, 0, 12, 256, 256 ) };
        yield return new object[] { new Region( 37, -122, 12, 256, 256 ) };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
