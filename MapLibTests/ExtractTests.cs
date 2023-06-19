#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ExtractTests.cs
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

public class ExtractTests : TestBase
{
    [ Theory ]
    [ InlineData( "BingMaps", 0, 0, 0, 256, 256, 1, 4 ) ]
    [ InlineData( "BingMaps", 37, -122, 0, 20, 20, 1, 1 ) ]
    [ InlineData( "BingMaps", 37, -122, 0, 40, 80, 4, 1 ) ]
    [ InlineData( "BingMaps", 40, -122, 315, 40, 200, 4, 2 ) ]
    [ InlineData( "BingMaps", 40, -112, 315, 100, 200, 4, 4 ) ]
    [ InlineData( "OpenStreetMaps", 0, 0, 0, 256, 256, 1, 4 ) ]
    [ InlineData( "OpenStreetMaps", 37, -122, 0, 20, 20, 1, 1 ) ]
    [ InlineData( "OpenStreetMaps", 37, -122, 0, 40, 80, 4, 1 ) ]
    [ InlineData( "OpenStreetMaps", 40, -122, 315, 40, 200, 4, 2 ) ]
    [ InlineData( "OpenStreetMaps", 40, -112, 315, 100, 200, 4, 4 ) ]
    [ InlineData( "OpenTopoMaps", 0, 0, 0, 256, 256, 1, 4 ) ]
    [ InlineData( "OpenTopoMaps", 37, -122, 0, 20, 20, 1, 1 ) ]
    [ InlineData( "OpenTopoMaps", 37, -122, 0, 40, 80, 4, 1 ) ]
    [ InlineData( "OpenTopoMaps", 40, -122, 315, 40, 200, 4, 2 ) ]
    [ InlineData( "OpenTopoMaps", 40, -112, 315, 100, 200, 4, 4 ) ]
    [ InlineData( "GoogleMaps", 0, 0, 0, 256, 256, 1, 1 ) ]
    [ InlineData( "GoogleMaps", 37, -122, 0, 20, 20, 1, 1 ) ]
    [ InlineData( "GoogleMaps", 37, -122, 0, 40, 80, 4, 1 ) ]
    [ InlineData( "GoogleMaps", 40, -122, 315, 40, 200, 4, 1 ) ]
    [ InlineData( "GoogleMaps", 40, -112, 315, 100, 200, 4, 1 ) ]
    public async Task BasicExtract(
        string projectionName,
        float latitude,
        float longitude,
        float heading,
        float height,
        float width,
        int scale,
        int numFragments
    )
    {
        var projection = CreateAndAuthenticateProjection( projectionName );
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();
        projection.MaxRequestLatency = 0;

        var center = new MapPoint( projection, scale );
        center.SetLatLong( latitude, longitude );

        var request = new Region
        {
            Height = height,
            Width = width,
            CenterPoint = center,
            Heading = heading,
            Scale = scale
        };

        var result = await projection.LoadRegionAsync( request );
        result.Should().NotBeNull();
        result!.ImagesLoaded.Should().BeTrue();

        var numBlocks = result.Count();
        numBlocks.Should().Be( numFragments );
    }
}
