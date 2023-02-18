using J4JSoftware.J4JMapLibrary;
using FluentAssertions;

namespace MapLibTests;

public class CheckImages : TestBase
{
    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task BingMaps( TileImageData.Tile data )
    {
        var result = await GetFactory().CreateMapProjection("BingMaps", null);
        var projection = result.Projection as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.MapScale.Scale = data.Scale;

        var mapTile = await TiledFragment.CreateAsync( projection, data.TileX, data.TileY );

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.FragmentId}{projection.MapServer.ImageFileExtension}" );

        await CompareImageFileAsync(filePath, await mapTile.GetImageAsync());
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task OpenStreetMaps(TileImageData.Tile data)
    {
        var result = await GetFactory().CreateMapProjection("OpenStreetMaps", null);
        var projection = result.Projection as OpenStreetMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.MapScale.Scale = data.Scale;

        var mapTile = await TiledFragment.CreateAsync(projection, data.TileX, data.TileY);

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.FragmentId}{projection.MapServer.ImageFileExtension}" );

        await CompareImageFileAsync(filePath, await mapTile.GetImageAsync());
    }

    [ Theory ]
    [ ClassData( typeof( TileImageData ) ) ]
    public async Task OpenTopoMaps(TileImageData.Tile data)
    {
        var result = await GetFactory().CreateMapProjection("OpenTopoMaps", null);
        var projection = result.Projection as OpenTopoMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.MapScale.Scale = data.Scale;

        var mapTile = await TiledFragment.CreateAsync(projection, data.TileX, data.TileY);

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapTile.FragmentId}{projection.MapServer.ImageFileExtension}" );

        await CompareImageFileAsync( filePath, await mapTile.GetImageAsync(), true );
    }

    [Theory]
    [ClassData(typeof(StaticImageData))]
    public async Task GoogleMaps(StaticImageData.Region data)
    {
        var result = await GetFactory().CreateMapProjection("GoogleMaps", null);
        var projection = result.Projection as GoogleMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.MapScale.Scale = data.Scale;

        var viewport = new NormalizedViewport( projection )
        {
            CenterLatitude = data.Latitude,
            CenterLongitude = data.Longitude,
            RequestedHeight = data.Height,
            RequestedWidth = data.Width,
            Scale = data.Scale
        };

        var mapTile = new StaticFragment( projection, viewport );

        var filePath = Path.Combine(GetCheckImagesFolder(projection.Name),
                                    $"{data.FragmentId}{projection.MapServer.ImageFileExtension}");

        await CompareImageFileAsync(filePath, await mapTile.GetImageAsync(), true);
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