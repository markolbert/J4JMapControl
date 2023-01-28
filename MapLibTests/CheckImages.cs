using J4JMapLibrary;
using FluentAssertions;

namespace MapLibTests;

public class CheckImages : TestBase
{
    [Theory(Skip="image files already created")]
    [InlineData(1, 0, 0)]
    [InlineData(2, 0, 0)]
    [InlineData(5, 27, 48)]
    public async void CreateImage(int scale, int xTile, int yTile)
    {
        var factory = GetFactory();

        foreach( var projectionName in factory.ProjectionNames )
        {
            if( projectionName == "OpenTopoMaps" )
                Thread.Sleep( 1000 );

            var projection = await factory.CreateMapProjection( projectionName ) as ITiledProjection;
            projection.Should().NotBeNull();
            projection!.Initialized.Should().BeTrue();

            projection.Scale = scale;

            var mapTile = new MapTile( projection, xTile, yTile );
            var stream = await mapTile.GetImageAsync();

            stream.Should().NotBeNull();

            var filePath = Path.Combine( GetCheckImagesFolder( projectionName ),
                                         $"{mapTile.QuadKey}{projection.ImageFileExtension}" );

            await WriteImageFileAsync( filePath, stream! );
        }
    }

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(2, 0, 0)]
    [InlineData(5, 27, 48)]
    public async void GetImage(int scale, int xTile, int yTile)
    {
        var factory = GetFactory();

        foreach( var projectionName in factory.ProjectionNames )
        {
            if (projectionName == "OpenTopoMaps")
                Thread.Sleep(1000);

            var projection = await GetFactory().CreateMapProjection( projectionName ) as ITiledProjection;
            projection.Should().NotBeNull();
            projection!.Initialized.Should().BeTrue();

            projection.Scale = scale;

            var mapTile = new MapTile( projection, xTile, yTile );

            var filePath = Path.Combine( GetCheckImagesFolder( projectionName ),
                                         $"{mapTile.QuadKey}{projection.ImageFileExtension}" );

            var result = await CompareImageFileAsync( filePath, await mapTile.GetImageAsync() );
            result.Should().BeTrue();
        }
    }

    private string GetCheckImagesFolder(string projectionName)
    {
        var retVal = Environment.CurrentDirectory;

        for (var idx = 0; idx < 3; idx++)
        {
            retVal = Path.GetDirectoryName(retVal)!;
        }

        retVal = Path.Combine(retVal, "check-images", projectionName);
        Directory.CreateDirectory(retVal);

        return retVal;
    }

    private async Task WriteImageFileAsync(string filePath, MemoryStream stream)
    {
        await File.WriteAllBytesAsync(filePath, stream.ToArray());
    }

    private async Task<bool> CompareImageFileAsync(string filePath, MemoryStream? stream)
    {
        stream.Should().NotBeNull();

        var imageBytes = await File.ReadAllBytesAsync( filePath );
        var checkBytes = stream!.ToArray();

        imageBytes.Length.Should().Be( checkBytes.Length );

        for( var idx = 0; idx < imageBytes.Length; idx++ )
        {
            imageBytes[ idx ].Should().Be( checkBytes[ idx ], $"because data at {idx} should match" );
        }

        return true;
    }

}
