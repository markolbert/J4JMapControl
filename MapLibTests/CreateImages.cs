using J4JMapLibrary;
using FluentAssertions;

namespace MapLibTests;

public class CreateImages : TestBase
{
    [ Theory ]
    [ClassData(typeof(TileImageData))]
    public async Task BingMaps( int scale, int xTile, int yTile )
    {
        var projection = await GetFactory().CreateMapProjection( typeof( BingMapsProjection ) );
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale( scale );

        var mapTile = await MapTile.CreateAsync(projection, xTile, yTile, GetCancellationToken("BingMaps"));
        await WriteImageFileAsync(projection, mapTile);
    }

    [Theory]
    [ClassData(typeof(TileImageData))]
    public async Task OpenStreetMaps(int scale, int xTile, int yTile)
    {
        var projection = await GetFactory().CreateMapProjection(typeof(OpenStreetMapsProjection));
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var mapTile = await MapTile.CreateAsync(projection, xTile, yTile, GetCancellationToken("OpenStreetMaps"));
        await WriteImageFileAsync(projection, mapTile);
    }

    [Theory]
    [ClassData(typeof(TileImageData))]
    public async Task OpenTopoMaps(int scale, int xTile, int yTile)
    {
        var projection = await GetFactory().CreateMapProjection(typeof(OpenTopoMapsProjection));
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var mapTile = await MapTile.CreateAsync(projection, xTile, yTile, GetCancellationToken("OpenTopoMaps"));
        await WriteImageFileAsync(projection, mapTile, true);
    }

    private async Task WriteImageFileAsync( ITiledProjection projection, MapTile tile, bool sleep = false )
    {
        var stream = await tile.GetImageAsync();
        stream.Should().NotBeNull();

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{tile.QuadKey}{projection.ImageFileExtension}" );

        await File.WriteAllBytesAsync( filePath, stream!.ToArray() );

        if( sleep )
            Thread.Sleep( 5000 );
    }
}
