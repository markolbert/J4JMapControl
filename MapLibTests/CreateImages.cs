#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// CreateImages.cs
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

public class CreateImages : TestBase
{
    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task BingMaps( TileImageData.Tile data )
    {
        var projection = CreateAndAuthenticateProjection( "BingMaps" ) as ITiledProjection;
        projection.Should().NotBeNull();

        var mapBlock = await GetMapBlockAsync( projection!, data );
        await WriteImageFileAsync( projection!, mapBlock );
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task OpenStreetMaps( TileImageData.Tile data )
    {
        var projection = CreateAndAuthenticateProjection( "OpenStreetMaps" ) as ITiledProjection;
        projection.Should().NotBeNull();

        var mapBlock = await GetMapBlockAsync(projection!, data);
        await WriteImageFileAsync(projection!, mapBlock);
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task TopoMaps( TileImageData.Tile data )
    {
        var projection = CreateAndAuthenticateProjection( "OpenTopoMaps" ) as ITiledProjection;
        projection.Should().NotBeNull();

        var mapBlock = await GetMapBlockAsync(projection!, data);
        await WriteImageFileAsync(projection!, mapBlock);
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task GoogleMaps( TileImageData.Tile data )
    {
        var projection = CreateAndAuthenticateProjection( "GoogleMaps" ) as StaticProjection;
        projection.Should().NotBeNull();

        var mapBlock = await GetMapBlockAsync(projection!, data);
        await WriteImageFileAsync(projection!, mapBlock);
    }

    private async Task<MapBlock> GetMapBlockAsync(ITiledProjection projection, TileImageData.Tile data)
    {
        var request = Region.FromTileCoordinates(projection, data.TileX, data.TileY, data.Scale);
        var regionView = new TiledRegionView( projection );

        var loaded = await regionView.LoadRegionAsync( request ) as LoadedTiledRegion;
        loaded.Should().NotBeNull();
        loaded!.ImagesLoaded.Should().BeTrue();
        loaded.Blocks.Should().HaveCount(1);

        return loaded.Blocks[0].MapBlock;
    }

    private async Task<MapBlock> GetMapBlockAsync( StaticProjection projection, TileImageData.Tile data )
    {
        var request = Region.FromTileCoordinates( projection, data.TileX, data.TileY, data.Scale );
        var regionView = new StaticRegionView( projection );

        var loaded = await regionView.LoadRegionAsync( request ) as LoadedStaticRegion;
        loaded.Should().NotBeNull();
        loaded!.ImagesLoaded.Should().BeTrue();
        
        loaded.Block.Should().NotBeNull();
        return loaded.Block!;
    }

    private async Task WriteImageFileAsync( IProjection projection, MapBlock mapBlock )
    {
        mapBlock.ImageBytes.Should().BePositive();
        mapBlock.ImageData.Should().NotBeNull();

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapBlock.FragmentId}{projection.ImageFileExtension}" );

        await File.WriteAllBytesAsync( filePath, mapBlock.ImageData! );
    }
}
