using J4JMapLibrary;
using FluentAssertions;

namespace MapLibTests;

public class CheckImages : TestBase
{
    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async void BingMaps( int scale, int xTile, int yTile )
    {
        var projection = await GetFactory().CreateMapProjection( typeof( BingMapsProjection ) );
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.Scale = scale;

        var mapTile = new MapTile( projection, xTile, yTile );

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.QuadKey}{projection.ImageFileExtension}" );

        await CompareImageFileAsync(filePath, await mapTile.GetImageAsync());
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async void OpenStreetMaps( int scale, int xTile, int yTile )
    {
        var projection = await GetFactory().CreateMapProjection( typeof( OpenStreetMapsProjection ) );
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.Scale = scale;

        var mapTile = new MapTile( projection, xTile, yTile );

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.QuadKey}{projection.ImageFileExtension}" );

        await CompareImageFileAsync(filePath, await mapTile.GetImageAsync());
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async void OpenTopoMaps( int scale, int xTile, int yTile )
    {
        var projection = await GetFactory().CreateMapProjection( typeof( OpenTopoMapsProjection ) );
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.Scale = scale;

        var mapTile = new MapTile( projection, xTile, yTile );

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.QuadKey}{projection.ImageFileExtension}" );

        await CompareImageFileAsync( filePath, await mapTile.GetImageAsync(), true );
    }

    private async Task CompareImageFileAsync( string filePath, MemoryStream? stream, bool sleep = true )
    {
        stream.Should().NotBeNull();

        var imageBytes = await File.ReadAllBytesAsync( filePath );
        var checkBytes = stream!.ToArray();

        imageBytes.Length.Should().Be( checkBytes.Length );

        for( var idx = 0; idx < imageBytes.Length; idx++ )
        {
            imageBytes[ idx ].Should().Be( checkBytes[ idx ], $"because data at {idx} should match" );
        }

        if( sleep )
            Thread.Sleep( 5000 );
    }
}