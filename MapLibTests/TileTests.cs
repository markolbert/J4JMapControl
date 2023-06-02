#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// TileTests.cs
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
using J4JSoftware.J4JMapLibrary.MapRegion;

namespace MapLibTests;

public class TileTests : TestBase
{
    [ Theory ]
    [ InlineData( "BingMaps", 1, 0, 0, 128, 256, 0, 0, 0, 1, 1 ) ]
    [ InlineData( "OpenStreetMaps", 0, 0, 0, 128, 256, 0, 0, 0, 0, 0 ) ]
    [ InlineData( "BingMaps", 2, 37, -122, 128, 256, 0, 0, 1, 1, 1 ) ]
    [ InlineData( "BingMaps", 2, 37, -122, 128, 256, 45, 0, 1, 1, 2 ) ]
    [ InlineData( "BingMaps", 2, 37, -122, 128, 512, 45, 0, 0, 3, 2 ) ]
    [ InlineData( "BingMaps", 3, 37, -122, 512, 512, 45, 0, 1, 7, 4 ) ]
    [ InlineData( "BingMaps", 3, 37, -122, 512, 512, 75, 0, 1, 2, 4 ) ]
    [ InlineData( "BingMaps", 3, 37, 97, 512, 512, 75, 4, 1, 7, 4 ) ]
    [ InlineData( "BingMaps", 3, 37, 97, 512, 512, 0, 5, 2, 7, 4 ) ]
    public async Task TileRegion(
        string projectionName,
        int scale,
        float latitude,
        float longitude,
        int height,
        int width,
        float heading,
        int minTileX,
        int minTileY,
        int maxTileX,
        int maxTileY
    )
    {
        var projection = CreateAndAuthenticateProjection( projectionName );
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var region = new MapRegion( projection, LoggerFactory )
                    .Center( latitude, longitude )
                    .Size( height, width )
                    .Heading( heading )
                    .Scale( scale )
                    .Update();

        var result = await projection.LoadRegionAsync( region );
        result.Should().BeTrue();

        var numTiles = region.TilesHigh * region.TilesWide;
        numTiles.Should().BeGreaterThan( 0 );

        region.Min( b => b.MapBlock?.X ?? int.MinValue ).Should().Be( minTileX );
        region.Min( b => b.MapBlock?.Y ?? int.MinValue ).Should().Be( minTileY );
        region.Max( b => b.MapBlock?.X ?? int.MaxValue ).Should().Be( maxTileX );
        region.Max( b => b.MapBlock?.Y ?? int.MaxValue ).Should().Be( maxTileY );

        foreach( var positionedBlock in region )
        {
            positionedBlock.MapBlock?.ImageBytes.Should().BePositive();
        }
    }

    [ Theory ]
    [ InlineData( 0, 4, 2, false, 0, 0 ) ]
    [ InlineData( 0, 4, 2, false, -1, 3 ) ]
    [ InlineData( 0, 4, 2, false, -2, 2 ) ]
    [ InlineData( 0, 3, 2, false, -2, 2 ) ]
    [ InlineData( 0, 3, 2, false, -1, 3 ) ]
    [ InlineData( 0, 4, 2, true, 15, -1 ) ]
    [ InlineData( 0, 3, 2, true, 15, -1 ) ]
    [ InlineData( 1, 2, 2, true, -15, -1 ) ]
    public void AbsoluteTile(
        int regionStart,
        int regionWidth,
        int scale,
        bool blockIsNull,
        int relativeX,
        int absoluteX
    )
    {
        var projection = CreateAndAuthenticateProjection( "BingMaps" );
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var width = regionWidth * projection.TileHeightWidth;

        var region = new MapRegion( projection, LoggerFactory )
                    .Size( projection.TileHeightWidth, width )
                    .Scale( scale );

        // since y tile is always 0, center is halfway down the first row
        var center = new MapPoint( region );
        center.SetCartesian( regionStart * projection.TileHeightWidth + width / 2, projection.TileHeightWidth / 2 );

        region.Center( center.Latitude, center.Longitude )
              .Update();

        var mapBlock = TileBlock.CreateBlock( region, relativeX, 0 );

        if( blockIsNull )
            mapBlock.Should().BeNull();
        else
        {
            mapBlock.Should().NotBeNull();
            mapBlock!.X.Should().Be( absoluteX );
            mapBlock.Y.Should().Be( 0 );
        }
    }
}