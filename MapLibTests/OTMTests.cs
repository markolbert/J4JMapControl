using FluentAssertions;
using J4JMapLibrary;

namespace MapLibTests;

public class OTMTests : TestBase
{
    [ Fact ]
    public async void ValidApiKey()
    {
        var projection = await GetFactory().CreateMapProjection("OpenTopoMaps") as OpenTopoMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();
    }

    [ Theory ]
    [ InlineData( 1, 0, 0 ) ]
    [ InlineData( 2, 0, 0 ) ]
    [InlineData(10, 27, 48)]
    public async void GetTile( int scale, int xTile, int yTile )
    {
        var projection = await GetFactory().CreateMapProjection("OpenTopoMaps") as OpenTopoMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.Scale = scale;

        var mapTile = new MapTile( projection, xTile, yTile );
        var stream = await mapTile.GetImageAsync();

        stream.Should().NotBeNull();
    }

    [ Theory ]
    [InlineData(1, 0, 0, "0")]
    [InlineData(2, 0, 0, "00")]
    [InlineData(2, 1, 0, "01")]
    [InlineData(2, 0, 1, "02")]
    [InlineData(2, 1, 1, "03")]
    [InlineData(3, 0, 0, "000")]
    [InlineData(3, 1, 0, "001")]
    [InlineData(3, 2, 0, "010")]
    [InlineData(3, 3, 0, "011")]
    [InlineData(3, 0, 1, "002")]
    [InlineData(3, 1, 1, "003")]
    [InlineData(3, 2, 1, "012")]
    [InlineData(3, 3, 1, "013")]
    [InlineData(3, 0, 2, "020")]
    [InlineData(3, 1, 2, "021")]
    [InlineData(3, 2, 2, "030")]
    [InlineData(3, 3, 2, "031")]
    [InlineData(3, 0, 3, "022")]
    [InlineData(3, 1, 3, "023")]
    [InlineData(3, 2, 3, "032")]
    [InlineData(3, 3, 3, "033")]
    public async void CreateQuadKeys( int scale, int xTile, int yTile, string quadKey )
    {
        var projection = await GetFactory().CreateMapProjection( "OpenTopoMaps" ) as OpenTopoMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.Scale = scale;

        var mapTile = new MapTile( projection, xTile, yTile );
        mapTile.ToQuadKey().Should().Be( quadKey );
    }
}