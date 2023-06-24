#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// RegionRetrieval.cs
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

public class RegionRetrieval : TestBase
{
    [ Theory ]
    [ClassData(typeof(RotatedRegion))]
    public async Task RotatedRegionRetrieval( RotatedRegion.Data data )
    {
        var projection = CreateAndAuthenticateProjection("BingMaps") as ITiledProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var width = data.ColumnsWide * projection.TileHeightWidth;
        var height = data.RowsHigh * projection.TileHeightWidth;

        var center = new MapPoint(projection, data.Scale);
        center.SetCartesian( ( data.CenterColumn + 0.5f ) * projection.TileHeightWidth,
                             ( data.CenterRow + 0.5f ) * projection.TileHeightWidth / 2 );

        var request = new Region
        {
            Height = height,
            Width = width,
            CenterPoint = center,
            Heading = ( 360 - data.Rotation ) % 360,
            Scale = data.Scale
        };

        var loaded = await projection.LoadRegionAsync(request) as TiledMapRegion;
        loaded.Should().NotBeNull();
        loaded!.ImagesLoaded.Should().BeTrue();

        loaded.Blocks.Should().HaveCount(data.Results.Length);

        for (var idx = 0; idx < data.Results.Length; idx++)
        {
            var positionedBlock = loaded.Blocks[idx];
            var result = data.Results[idx];

            positionedBlock.RegionColumn.Should().Be(result.RegionColumn);
            positionedBlock.RegionRow.Should().Be(result.Row);

            positionedBlock.MapBlock.AbsoluteColumn.Should().Be(result.AbsoluteColumn);
            positionedBlock.MapBlock.AbsoluteRow.Should().Be(result.Row);
        }
    }

    [ Theory ]
    [ ClassData( typeof( HorizontalRegion ) ) ]
    public async Task HorizontalStripRetrieval( HorizontalRegion.Data data )
    {
        var projection = CreateAndAuthenticateProjection( "BingMaps" ) as ITiledProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var width = data.ColumnsWide * projection.TileHeightWidth;

        // since y tile is always 0, center is halfway down the first row
        var center = new MapPoint( projection, data.Scale );
        center.SetCartesian( ( data.CenterColumn + 0.5f ) * projection.TileHeightWidth,
                             projection.TileHeightWidth / 2 );

        var request = new Region
        {
            Height = projection.TileHeightWidth,
            Width = width,
            CenterPoint = center,
            Heading = 0,
            Scale = data.Scale
        };

        var loaded = await projection.LoadRegionAsync( request ) as TiledMapRegion;
        loaded.Should().NotBeNull();
        loaded!.ImagesLoaded.Should().BeTrue();

        loaded.Blocks.Should().HaveCount( data.Results.Length );

        for( var idx = 0; idx < data.Results.Length; idx++ )
        {
            var positionedBlock = loaded.Blocks[ idx ];
            var result = data.Results[ idx ];

            positionedBlock.RegionColumn.Should().Be( result.RegionColumn );
            positionedBlock.RegionRow.Should().Be( 0 );

            positionedBlock.MapBlock.AbsoluteColumn.Should().Be( result.AbsoluteColumn );
            positionedBlock.MapBlock.AbsoluteRow.Should().Be( 0 );
        }
    }
}
