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

using J4JSoftware.J4JMapLibrary;
using FluentAssertions;
using J4JSoftware.J4JMapLibrary.MapRegion;

namespace MapLibTests;

public class CreateImages : TestBase
{
    [ Theory ]
    [ClassData(typeof(TileImageData))]
    public async Task Create( TileImageData.Tile data )
    {
        await WriteImageFileAsync("BingMaps", data);
        await WriteImageFileAsync("GoogleMaps", data);
        await WriteImageFileAsync("OpenStreetMaps", data);
        await WriteImageFileAsync("OpenTopoMaps", data);
    }

    private async Task WriteImageFileAsync( string projName, TileImageData.Tile data )
    {
        var projection = CreateAndAuthenticateProjection( projName );
        projection.Should().NotBeNull();

        var mapRegion = new MapRegion(projection!, LoggerFactory)
                       .Scale(data.Scale)
                       .Update();

        var mapTile = projName == "GoogleMaps"
            ? MapTile.CreateStaticMapTile( projection!, data.TileX, data.TileY, data.Scale, LoggerFactory )
            : new MapTile( mapRegion, data.TileY ).SetXAbsolute( data.TileX );

        var loaded = await projection!.LoadImageAsync(mapTile);
        loaded.Should().BeTrue();
        mapTile.ImageBytes.Should().BePositive();
        mapTile.ImageData.Should().NotBeNull();

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.FragmentId}{projection.ImageFileExtension}" );

        await File.WriteAllBytesAsync( filePath, mapTile.ImageData! );
    }
}
