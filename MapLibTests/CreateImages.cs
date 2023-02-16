using J4JSoftware.J4JMapLibrary;
using FluentAssertions;

namespace MapLibTests;

public class CreateImages : TestBase
{
    [ Theory ]
    [ClassData(typeof(TileImageData))]
    public async Task BingMaps( TileImageData.Tile data )
    {
        var result = await GetFactory().CreateMapProjection( "BingMaps", null );
        var projection = result.Projection as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.MapScale.Scale = data.Scale;

        var mapTile = await TiledFragment.CreateAsync(projection, data.TileX, data.TileY, data.Scale);
        await WriteImageFileAsync(projection, mapTile,data.Scale);
    }

    [Theory]
    [ClassData(typeof(TileImageData))]
    public async Task OpenStreetMaps(TileImageData.Tile data)
    {
        var result = await GetFactory().CreateMapProjection("OpenStreetMaps", null);
        var projection = result.Projection as OpenStreetMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.MapScale.Scale = data.Scale;

        var mapTile = await TiledFragment.CreateAsync(projection, data.TileX, data.TileY, data.Scale);
        await WriteImageFileAsync(projection, mapTile, data.Scale);
    }

    [Theory]
    [ClassData(typeof(TileImageData))]
    public async Task OpenTopoMaps(TileImageData.Tile data)
    {
        var result = await GetFactory().CreateMapProjection("OpenTopoMaps", null);
        var projection = result.Projection as OpenTopoMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.MapScale.Scale = data.Scale;

        var mapTile = await TiledFragment.CreateAsync(projection, data.TileX, data.TileY, data.Scale);
        await WriteImageFileAsync(projection, mapTile, data.Scale);
    }

    [ Theory ]
    [ ClassData( typeof( StaticImageData ) ) ]
    public async Task GoogleMaps( StaticImageData.Region data )
    {
        var result = await GetFactory().CreateMapProjection( "GoogleMaps", null );
        var projection = result.Projection as GoogleMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var mapTile = new StaticFragment( projection,
                                          data.Latitude,
                                          data.Longitude,
                                          data.Height,
                                          data.Width,
                                          data.Scale );

        await WriteImageFileAsync( projection, mapTile, data, true );
    }

    private async Task WriteImageFileAsync(
        ITiledProjection projection,
        TiledFragment mapFragment,
        int scale,
        bool sleep = false
    )
    {
        var stream = await mapFragment.GetImageAsync( scale );
        stream.Should().NotBeNull();

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{mapFragment.FragmentId}{projection.MapServer.ImageFileExtension}" );

        await File.WriteAllBytesAsync( filePath, stream!.ToArray() );

        if( sleep )
            Thread.Sleep( 5000 );
    }

    private async Task WriteImageFileAsync(
        IProjection projection,
        StaticFragment mapFragment,
        StaticImageData.Region data,
        bool sleep = false
    )
    {
        var stream = await mapFragment.GetImageAsync( data.Scale );
        stream.Should().NotBeNull();

        var filePath = Path.Combine( GetCheckImagesFolder( projection.Name ),
                                     $"{data.FragmentId}{projection.MapServer.ImageFileExtension}" );

        await File.WriteAllBytesAsync( filePath, stream!.ToArray() );

        if( sleep )
            Thread.Sleep( 5000 );
    }
}
