using J4JSoftware.J4JMapLibrary;
using FluentAssertions;

namespace MapLibTests;

public class CheckImages : TestBase
{
    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task BingMaps( TileImageData.Tile data )
    {
        var projection = await CreateProjection( "BingMaps" ) as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapTile = await projection.GetMapTileByRegionCoordinatesAsync( data.TileX, data.TileY, data.Scale );
        mapTile.Should().NotBeNull();
        mapTile.ImageBytes.Should().BePositive();
        mapTile.ImageData.Should().NotBeNull();

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.FragmentId}{projection.ImageFileExtension}" );

        await CompareImageFileAsync( filePath, await mapTile.GetImageAsync() );
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task OpenStreetMaps( TileImageData.Tile data )
    {
        var projection = await CreateProjection( "OpenStreetMaps" ) as OpenStreetMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapTile = await projection.GetMapTileByRegionCoordinatesAsync( data.TileX, data.TileY, data.Scale );
        mapTile.Should().NotBeNull();
        mapTile.ImageBytes.Should().BePositive();
        mapTile.ImageData.Should().NotBeNull();

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.FragmentId}{projection.ImageFileExtension}" );

        await CompareImageFileAsync( filePath, await mapTile.GetImageAsync() );
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task OpenTopoMaps( TileImageData.Tile data )
    {
        var projection = await CreateProjection( "OpenTopoMaps" ) as OpenTopoMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapTile = await projection.GetMapTileByRegionCoordinatesAsync( data.TileX, data.TileY, data.Scale );
        mapTile.Should().NotBeNull();
        mapTile.ImageBytes.Should().BePositive();
        mapTile.ImageData.Should().NotBeNull();

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.FragmentId}{projection.ImageFileExtension}" );

        await CompareImageFileAsync( filePath, await mapTile.GetImageAsync() );
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task GoogleMaps( TileImageData.Tile data )
    {
        var projection = await CreateProjection( "GoogleMaps" ) as GoogleMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapTile = await projection.GetMapTileByProjectionCoordinatesAsync( data.TileX, data.TileY, data.Scale );
        mapTile.Should().NotBeNull();
        mapTile.ImageBytes.Should().BePositive();
        mapTile.ImageData.Should().NotBeNull();

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.FragmentId}{projection.ImageFileExtension}" );

        await CompareImageFileAsync( filePath, await mapTile.GetImageAsync() );
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