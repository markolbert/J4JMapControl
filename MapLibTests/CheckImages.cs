#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// CheckImages.cs
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

public class CheckImages : TestBase
{
    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task BingMaps( TileImageData.Tile data )
    {
        var projection = CreateAndAuthenticateProjection( "BingMaps" ) as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapBlock = await projection.GetMapTileAsync( data.TileX, data.TileY, data.Scale );
        mapBlock.Should().NotBeNull();
        mapBlock!.ImageBytes.Should().BePositive();
        mapBlock.ImageData.Should().NotBeNull();

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapBlock.FragmentId}{projection.ImageFileExtension}" );

        await CompareImageFileAsync( filePath, await projection.GetImageAsync( mapBlock ) );
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task OpenStreetMaps( TileImageData.Tile data )
    {
        var projection = CreateAndAuthenticateProjection( "OpenStreetMaps" ) as OpenStreetMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapBlock = await projection.GetMapTileAsync( data.TileX, data.TileY, data.Scale );
        mapBlock.Should().NotBeNull();
        mapBlock!.ImageBytes.Should().BePositive();
        mapBlock.ImageData.Should().NotBeNull();

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapBlock.FragmentId}{projection.ImageFileExtension}" );

        await CompareImageFileAsync( filePath, await projection.GetImageAsync( mapBlock ) );
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task OpenTopoMaps( TileImageData.Tile data )
    {
        var projection = CreateAndAuthenticateProjection( "OpenTopoMaps" ) as OpenTopoMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapBlock = await projection.GetMapTileAsync( data.TileX, data.TileY, data.Scale );
        mapBlock.Should().NotBeNull();
        mapBlock!.ImageBytes.Should().BePositive();
        mapBlock.ImageData.Should().NotBeNull();

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapBlock.FragmentId}{projection.ImageFileExtension}" );

        await CompareImageFileAsync( filePath, await projection.GetImageAsync( mapBlock ) );
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task GoogleMaps( TileImageData.Tile data )
    {
        var projection = CreateAndAuthenticateProjection( "GoogleMaps" ) as GoogleMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapBlock = await projection.GetMapTileAsync( data.TileX, data.TileY, data.Scale );
        mapBlock.Should().NotBeNull();
        mapBlock!.ImageBytes.Should().BePositive();
        mapBlock.ImageData.Should().NotBeNull();

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapBlock.FragmentId}{projection.ImageFileExtension}" );

        await CompareImageFileAsync( filePath, await projection.GetImageAsync( mapBlock ) );
    }

    private async Task CompareImageFileAsync( string filePath, byte[]? imageData )
    {
        imageData.Should().NotBeNull();

        var imageBytes = await File.ReadAllBytesAsync( filePath );
        var checkBytes = imageData!.ToArray();

        imageBytes.Length.Should().Be( checkBytes.Length );

        for( var idx = 0; idx < imageBytes.Length; idx++ )
        {
            imageBytes[ idx ].Should().Be( checkBytes[ idx ], $"because data at {idx} should match" );
        }
    }
}
