﻿using J4JMapLibrary;
using FluentAssertions;

namespace MapLibTests;

public class CreateImages : TestBase
{
    [ Theory ]
    [ClassData(typeof(TileImageData))]
    public async Task BingMaps( int scale, int xTile, int yTile )
    {
        var projection = await GetFactory().CreateMapProjection( "BingMaps" ) as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale( scale );

        var mapTile = await FixedMapTile.CreateAsync(projection, xTile, yTile, GetCancellationToken(500));
        await WriteImageFileAsync(projection, mapTile);
    }

    [Theory]
    [ClassData(typeof(TileImageData))]
    public async Task OpenStreetMaps(int scale, int xTile, int yTile)
    {
        var projection = await GetFactory().CreateMapProjection("OpenStreetMaps") as OpenStreetMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var mapTile = await FixedMapTile.CreateAsync(projection, xTile, yTile, GetCancellationToken(500));
        await WriteImageFileAsync(projection, mapTile);
    }

    [Theory]
    [ClassData(typeof(TileImageData))]
    public async Task OpenTopoMaps(int scale, int xTile, int yTile)
    {
        var projection = await GetFactory().CreateMapProjection("OpenTopoMaps") as OpenTopoMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var mapTile = await FixedMapTile.CreateAsync(projection, xTile, yTile, GetCancellationToken(500));
        await WriteImageFileAsync(projection, mapTile, true);
    }

    private async Task WriteImageFileAsync( IFixedTileProjection projection, FixedMapTile tile, bool sleep = false )
    {
        var stream = await tile.GetImageAsync();
        stream.Should().NotBeNull();

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{tile.QuadKey}{projection.MapServer.ImageFileExtension}" );

        await File.WriteAllBytesAsync( filePath, stream!.ToArray() );

        if( sleep )
            Thread.Sleep( 5000 );
    }
}
