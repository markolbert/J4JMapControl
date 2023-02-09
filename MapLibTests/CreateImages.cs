using J4JMapLibrary;
using FluentAssertions;

namespace MapLibTests;

public class CreateImages : TestBase
{
    [ Theory ]
    [ClassData(typeof(TileImageData))]
    public async Task BingMaps( int scale, int xTile, int yTile )
    {
        var result = await GetFactory().CreateMapProjection( "BingMaps", null );
        var projection = result.Projection as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale( scale );

        var mapTile = await FixedMapTile.CreateAsync(projection, xTile, yTile, GetCancellationToken());
        await WriteImageFileAsync(projection, mapTile);
    }

    [Theory]
    [ClassData(typeof(TileImageData))]
    public async Task OpenStreetMaps(int scale, int xTile, int yTile)
    {
        var result = await GetFactory().CreateMapProjection("OpenStreetMaps", null);
        var projection = result.Projection as OpenStreetMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var mapTile = await FixedMapTile.CreateAsync(projection, xTile, yTile, GetCancellationToken());
        await WriteImageFileAsync(projection, mapTile);
    }

    [Theory]
    [ClassData(typeof(TileImageData))]
    public async Task OpenTopoMaps(int scale, int xTile, int yTile)
    {
        var result = await GetFactory().CreateMapProjection("OpenTopoMaps", null);
        var projection = result.Projection as OpenTopoMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var mapTile = await FixedMapTile.CreateAsync(projection, xTile, yTile, GetCancellationToken());
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
