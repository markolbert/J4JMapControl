﻿using J4JMapLibrary;
using FluentAssertions;

namespace MapLibTests;

public class CheckImages : TestBase
{
    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task BingMaps( int scale, int xTile, int yTile )
    {
        var result = await GetFactory().CreateMapProjection("BingMaps", null);
        var projection = result.Projection as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var mapTile = await TiledFragment.CreateAsync( projection, xTile, yTile );

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.QuadKey}{projection.MapServer.ImageFileExtension}" );

        await CompareImageFileAsync(filePath, await mapTile.GetImageAsync());
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task OpenStreetMaps( int scale, int xTile, int yTile )
    {
        var result = await GetFactory().CreateMapProjection("OpenStreetMaps", null);
        var projection = result.Projection as OpenStreetMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var mapTile = await TiledFragment.CreateAsync(projection, xTile, yTile);

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.QuadKey}{projection.MapServer.ImageFileExtension}" );

        await CompareImageFileAsync(filePath, await mapTile.GetImageAsync());
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task OpenTopoMaps( int scale, int xTile, int yTile )
    {
        var result = await GetFactory().CreateMapProjection("OpenTopoMaps", null);
        var projection = result.Projection as OpenTopoMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var mapTile = await TiledFragment.CreateAsync(projection, xTile, yTile);

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.QuadKey}{projection.MapServer.ImageFileExtension}" );

        await CompareImageFileAsync( filePath, await mapTile.GetImageAsync(), true );
    }

    private async Task CompareImageFileAsync( string filePath, byte[]? imageData, bool sleep = true )
    {
        imageData.Should().NotBeNull();

        var imageBytes = await File.ReadAllBytesAsync( filePath );
        var checkBytes = imageData!.ToArray();

        imageBytes.Length.Should().Be( checkBytes.Length );

        for( var idx = 0; idx < imageBytes.Length; idx++ )
        {
            imageBytes[ idx ].Should().Be( checkBytes[ idx ], $"because data at {idx} should match" );
        }

        if( sleep )
            Thread.Sleep( 5000 );
    }
}