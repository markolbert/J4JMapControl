using J4JSoftware.J4JMapLibrary;
using FluentAssertions;

namespace MapLibTests;

public class CreateImages : TestBase
{
    [ Theory ]
    [ClassData(typeof(TileImageData))]
    public async Task BingMaps( TileImageData.Tile data )
    {
        var projection = await CreateProjection("BingMaps") as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapTile = await projection.GetFragmentAsync( data.TileX, data.TileY, data.Scale ) as ITiledFragment;
        mapTile.Should().NotBeNull();
        mapTile!.ImageBytes.Should().BePositive();
        mapTile.ImageData.Should().NotBeNull();

        await WriteTiledImageFileAsync( projection, mapTile );
    }

    [Theory]
    [ClassData(typeof(TileImageData))]
    public async Task OpenStreetMaps(TileImageData.Tile data)
    {
        var projection = await CreateProjection("OpenStreetMaps") as OpenStreetMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapTile = await projection.GetFragmentAsync(data.TileX, data.TileY, data.Scale) as ITiledFragment;
        mapTile.Should().NotBeNull();
        mapTile!.ImageBytes.Should().BePositive();
        mapTile.ImageData.Should().NotBeNull();

        await WriteTiledImageFileAsync(projection, mapTile);
    }

    [Theory]
    [ClassData(typeof(TileImageData))]
    public async Task OpenTopoMaps(TileImageData.Tile data)
    {
        var projection = await CreateProjection("OpenTopoMaps") as OpenTopoMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapTile = await projection.GetFragmentAsync(data.TileX, data.TileY, data.Scale) as ITiledFragment;
        mapTile.Should().NotBeNull();
        mapTile!.ImageBytes.Should().BePositive();
        mapTile.ImageData.Should().NotBeNull();

        await WriteTiledImageFileAsync(projection, mapTile);
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task GoogleMaps( TileImageData.Tile data )
    {
        var projection = await CreateProjection("GoogleMaps") as GoogleMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapTile = await projection.GetFragmentAsync(data.TileX, data.TileY, data.Scale) as IStaticFragment;
        mapTile.Should().NotBeNull();
        mapTile!.ImageBytes.Should().BePositive();
        mapTile.ImageData.Should().NotBeNull();

        await WriteStaticImageFileAsync( projection, mapTile );
    }

    private async Task WriteTiledImageFileAsync(
        ITiledProjection projection,
        ITiledFragment mapFragment
    )
    {
        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapFragment.FragmentId}{projection.ImageFileExtension}" );

        await File.WriteAllBytesAsync( filePath, mapFragment.ImageData! );
    }

    private async Task WriteStaticImageFileAsync(
        IProjection projection,
        IStaticFragment mapFragment
    )
    {
        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapFragment.FragmentId}{projection.ImageFileExtension}" );

        await File.WriteAllBytesAsync( filePath, mapFragment.ImageData! );
    }
}
