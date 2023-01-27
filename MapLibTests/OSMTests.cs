using FluentAssertions;
using J4JMapLibrary;

namespace MapLibTests;

public class OSMTests : TestBase
{
    [ Fact ]
    public async void ValidApiKey()
    {
        var projection = await GetFactory().CreateMapProjection("OpenStreetMaps") as OpenStreetMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();
    }

    [ Theory ]
    [ InlineData( 1, 0, 0 ) ]
    [ InlineData( 2, 0, 0 ) ]
    [InlineData(15, 27, 48)]
    public async void GetTile( int scale, int xTile, int yTile )
    {
        var projection = await GetFactory().CreateMapProjection("OpenStreetMaps") as OpenStreetMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.Scale = scale;

        var mapTile = new MapTile( projection, xTile, yTile );
        var stream = await mapTile.GetImageAsync();

        stream.Should().NotBeNull();
    }

    [ Theory ]
    [ InlineData( 1, 0, 0, "0" ) ]
    [ InlineData( 2, 0, 0, "0" ) ]
    [ InlineData( 2, 1, 0, "1" ) ]
    [ InlineData( 2, 0, 1, "2" ) ]
    [ InlineData( 2, 1, 1, "3" ) ]
    [ InlineData( 3, 0, 0, "00" ) ]
    [ InlineData( 3, 1, 0, "01" ) ]
    [ InlineData( 3, 2, 0, "10" ) ]
    [ InlineData( 3, 3, 0, "11" ) ]
    [ InlineData( 3, 0, 1, "02" ) ]
    [ InlineData( 3, 1, 1, "03" ) ]
    [ InlineData( 3, 2, 1, "12" ) ]
    [ InlineData( 3, 3, 1, "13" ) ]
    [ InlineData( 3, 0, 2, "20" ) ]
    [ InlineData( 3, 1, 2, "21" ) ]
    [ InlineData( 3, 2, 2, "30" ) ]
    [ InlineData( 3, 3, 2, "31" ) ]
    [ InlineData( 3, 0, 3, "22" ) ]
    [ InlineData( 3, 1, 3, "23" ) ]
    [ InlineData( 3, 2, 3, "32" ) ]
    [ InlineData( 3, 3, 3, "33" ) ]
    [ InlineData( 4, 0, 0, "000" ) ]
    [ InlineData( 4, 3, 2, "031" ) ]
    [ InlineData( 4, 4, 6, "320" ) ]
    public async void TestQuadKeys( int scale, int xTile, int yTile, string quadKey )
    {
        var projection = await GetFactory().CreateMapProjection( "OpenStreetMaps" ) as OpenStreetMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.Scale = scale;

        var mapTile = new MapTile( projection, xTile, yTile );
        mapTile.GetQuadKey().Should().Be( quadKey );
    }
}