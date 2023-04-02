using J4JSoftware.J4JMapLibrary;
using FluentAssertions;
using J4JSoftware.J4JMapLibrary.MapRegion;

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

        var mapRegion = new MapRegion( projection, Logger )
                       .Scale( data.Scale )
                       .Update();

        var mapTile = new MapTile( mapRegion, data.TileY ).SetXAbsolute( data.TileX );

        var loaded = await mapTile.LoadImageAsync();
        loaded.Should().BeTrue();
        mapTile.ImageBytes.Should().BePositive();
        mapTile.ImageData.Should().NotBeNull();

        await WriteImageFileAsync( projection, mapTile );
    }

    [Theory]
    [ClassData(typeof(TileImageData))]
    public async Task OpenStreetMaps(TileImageData.Tile data)
    {
        var projection = await CreateProjection("OpenStreetMaps") as OpenStreetMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapRegion = new MapRegion(projection, Logger)
                       .Scale(data.Scale)
                       .Update();

        var mapTile = new MapTile( mapRegion, data.TileY ).SetXAbsolute( data.TileX );

        var loaded = await mapTile.LoadImageAsync();
        loaded.Should().BeTrue();
        mapTile.ImageBytes.Should().BePositive();
        mapTile.ImageData.Should().NotBeNull();

        await WriteImageFileAsync(projection, mapTile);
    }

    [Theory]
    [ClassData(typeof(TileImageData))]
    public async Task OpenTopoMaps(TileImageData.Tile data)
    {
        var projection = await CreateProjection("OpenTopoMaps") as OpenTopoMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapRegion = new MapRegion(projection, Logger)
                       .Scale(data.Scale)
                       .Update();

        var mapTile = new MapTile( mapRegion, data.TileY ).SetXAbsolute( data.TileX );

        var loaded = await mapTile.LoadImageAsync();
        loaded.Should().BeTrue();
        mapTile.ImageBytes.Should().BePositive();
        mapTile.ImageData.Should().NotBeNull();

        await WriteImageFileAsync(projection, mapTile);
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task GoogleMaps( TileImageData.Tile data )
    {
        var projection = await CreateProjection("GoogleMaps") as GoogleMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapTile = MapTile.CreateStaticMapTile( projection, data.TileX, data.TileY, data.Scale, Logger );
        
        var loaded = await mapTile.LoadImageAsync();
        loaded.Should().BeTrue();
        mapTile.ImageBytes.Should().BePositive();
        mapTile.ImageData.Should().NotBeNull();

        await WriteImageFileAsync( projection, mapTile );
    }

    private async Task WriteImageFileAsync(
        IProjection projection,
        MapTile mapTile
    )
    {
        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.FragmentId}{projection.ImageFileExtension}" );

        await File.WriteAllBytesAsync( filePath, mapTile.ImageData! );
    }
}
