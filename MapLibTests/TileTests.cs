﻿#region copyright

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
        int minCol,
        int minRow,
        int maxCol,
        int maxRow
    )
    {
        var projection = CreateAndAuthenticateProjection( projectionName );
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var request = new Region
        {
            Height = height,
            Width = width,
            Latitude = latitude,
            Longitude = longitude,
            Heading = heading,
            Scale = scale
        };

        var regionView = projection switch
        {
            ITiledProjection tiledProj => (IRegionView?)new TiledRegionView(tiledProj),
            StaticProjection staticProj => new StaticRegionView(staticProj),
            _ => null
        };

        regionView.Should().NotBeNull();

        var result = await regionView!.LoadRegionAsync(request);
        result.Succeeded.Should().BeTrue();

        var numTiles = projection.GetNumTiles( scale );
        numTiles *= numTiles;
        numTiles.Should().BeGreaterThan( 0 );

        switch( regionView )
        {
            case TiledRegionView tiledView:
                tiledView.PositionedBlocks.Min( b => b.MapBlock.ProjectionCoordinates.Column )
                         .Should().Be( minCol );

                tiledView.PositionedBlocks.Min(b => b.MapBlock.ProjectionCoordinates.Row)
                         .Should().Be(minRow);

                tiledView.PositionedBlocks.Max(b => b.MapBlock.ProjectionCoordinates.Column)
                         .Should().Be(maxCol);

                tiledView.PositionedBlocks.Max(b => b.MapBlock.ProjectionCoordinates.Row)
                         .Should().Be(maxRow);

                foreach( var block in tiledView.PositionedBlocks.Select( b => b.MapBlock ) )
                {
                    block.ImageBytes.Should().BePositive();
                }

                break;

            case StaticRegionView staticView:
                staticView.StaticBlock.Should().NotBeNull();
                staticView.StaticBlock!.ImageBytes.Should().BePositive();
                break;

            default:
                false.Should().BeTrue();
                break;
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
    public async Task AbsoluteTile(
        int regionStart,
        int regionWidth,
        int scale,
        bool blockIsNull,
        int relativeCol,
        int absoluteCol
    )
    {
        var projection = CreateAndAuthenticateProjection( "BingMaps" ) as ITiledProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var width = regionWidth * projection.TileHeightWidth;

        // since y tile is always 0, center is halfway down the first row
        var center = new MapPoint(projection, scale);
        center.SetCartesian(regionStart * projection.TileHeightWidth + width / 2, projection.TileHeightWidth / 2);

        var request = new Region
        {
            Height = projection.TileHeightWidth,
            Width = projection.TileHeightWidth,
            Latitude = center.Latitude,
            Longitude = center.Longitude,
            Heading = 0,
            Scale = scale
        };

        var region = new TiledRegionView( projection );
        var loaded = await region.LoadRegionAsync( request );
        loaded.Succeeded.Should().BeTrue();

        if( blockIsNull )
            region.PositionedBlocks.Should().HaveCount( 0 );
        else
        {
            region.PositionedBlocks.Should().HaveCount( 1 );

            var positionedBlock = region.PositionedBlocks[ 0 ];
            positionedBlock.Column.Should().Be( relativeCol );
            positionedBlock.Row.Should().Be( relativeCol );

            positionedBlock.MapBlock.ProjectionCoordinates.Column.Should().Be( absoluteCol );
            positionedBlock.MapBlock.ProjectionCoordinates.Row.Should().Be( 0 );
        }
    }
}
