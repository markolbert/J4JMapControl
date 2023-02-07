using J4JMapLibrary;
using FluentAssertions;

namespace MapLibTests;

public class CheckImages : TestBase
{
    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task BingMaps( int scale, int xTile, int yTile )
    {
        var projection = await GetFactory().CreateMapProjection( typeof( BingMapsProjection ) ) as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var mapTile = await FixedMapTile.CreateAsync( projection, xTile, yTile, GetCancellationToken("BingMaps") );

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.QuadKey}{projection.ImageFileExtension}" );

        await CompareImageFileAsync(filePath, await mapTile.GetImageAsync());
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task OpenStreetMaps( int scale, int xTile, int yTile )
    {
        var projection = await GetFactory().CreateMapProjection( typeof( OpenStreetMapsProjection ) );
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var mapTile = await FixedMapTile.CreateAsync(projection, xTile, yTile, GetCancellationToken("OpenStreetMaps"));

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.QuadKey}{projection.ImageFileExtension}" );

        await CompareImageFileAsync(filePath, await mapTile.GetImageAsync());
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task OpenTopoMaps( int scale, int xTile, int yTile )
    {
        var projection = await GetFactory().CreateMapProjection( typeof( OpenTopoMapsProjection ) );
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var mapTile = await FixedMapTile.CreateAsync(projection, xTile, yTile, GetCancellationToken("OpenTopoMaps"));

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.QuadKey}{projection.ImageFileExtension}" );

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