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
using J4JSoftware.J4JMapLibrary.MapRegion;

namespace MapLibTests;

public class CreateImages : TestBase
{
    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task BingMaps( TileImageData.Tile data )
    {
        var projection = CreateAndAuthenticateProjection( "BingMaps" );
        projection.Should().NotBeNull();

        var mapRegion = new MapRegion( projection!, LoggerFactory )
                       .Scale( data.Scale )
                       .Update();

        var mapBlock = TileBlock.CreateBlock( mapRegion, data.TileX, data.TileY );
        mapBlock.Should().NotBeNull();

        var loaded = await projection!.LoadImageAsync( mapBlock! );
        loaded.Should().BeTrue();
        mapBlock!.ImageBytes.Should().BePositive();
        mapBlock.ImageData.Should().NotBeNull();

        await WriteImageFileAsync( projection, mapBlock );
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task OpenStreetMaps( TileImageData.Tile data )
    {
        var projection = CreateAndAuthenticateProjection( "OpenStreetMaps" );
        projection.Should().NotBeNull();

        var mapRegion = new MapRegion( projection!, LoggerFactory )
                       .Scale( data.Scale )
                       .Update();

        var mapBlock = TileBlock.CreateBlock( mapRegion, data.TileX, data.TileY );
        mapBlock.Should().NotBeNull();

        var loaded = await projection!.LoadImageAsync( mapBlock! );
        loaded.Should().BeTrue();
        mapBlock!.ImageBytes.Should().BePositive();
        mapBlock.ImageData.Should().NotBeNull();

        await WriteImageFileAsync( projection, mapBlock );
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task TopoMaps( TileImageData.Tile data )
    {
        var projection = CreateAndAuthenticateProjection( "OpenTopoMaps" );
        projection.Should().NotBeNull();

        var mapRegion = new MapRegion( projection!, LoggerFactory )
                       .Scale( data.Scale )
                       .Update();

        var mapBlock = TileBlock.CreateBlock( mapRegion, data.TileX, data.TileY );
        mapBlock.Should().NotBeNull();

        var loaded = await projection!.LoadImageAsync( mapBlock! );
        loaded.Should().BeTrue();
        mapBlock!.ImageBytes.Should().BePositive();
        mapBlock.ImageData.Should().NotBeNull();

        await WriteImageFileAsync( projection, mapBlock );
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task GoogleMaps( TileImageData.Tile data )
    {
        var projection = CreateAndAuthenticateProjection( "GoogleMaps" );
        projection.Should().NotBeNull();

        var mapBlock = StaticBlock.CreateBlock( projection!, data.TileX, data.TileY, data.Scale );
        mapBlock.Should().NotBeNull();

        var loaded = await projection!.LoadImageAsync( mapBlock! );
        loaded.Should().BeTrue();
        mapBlock!.ImageBytes.Should().BePositive();
        mapBlock.ImageData.Should().NotBeNull();

        await WriteImageFileAsync( projection, mapBlock );
    }

    private async Task WriteImageFileAsync( IProjection projection, MapBlock mapBlock )
    {
        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapBlock.FragmentId}{projection.ImageFileExtension}" );

        await File.WriteAllBytesAsync( filePath, mapBlock.ImageData! );
    }
}
