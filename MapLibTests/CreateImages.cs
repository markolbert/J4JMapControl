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
