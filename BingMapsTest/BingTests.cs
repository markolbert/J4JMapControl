using FluentAssertions;
using J4JMapLibrary;
using J4JSoftware.DeusEx;
using Microsoft.Extensions.DependencyInjection;

namespace BingMapsTest;

public class BingTests : TestBase
{
    [Fact]
    public async void EmptyBingApiKey()
    {
        var bingMaps = J4JDeusEx.ServiceProvider.GetRequiredService<BingMapProjection>();
        var temp = await bingMaps.InitializeAsync( string.Empty, BingMapType.Aerial );

        temp.Should().Be( false );
    }

    [ Fact ]
    public async void ValidBingApiKey()
    {
        var bingMaps = J4JDeusEx.ServiceProvider.GetRequiredService<BingMapProjection>();
        var temp = await bingMaps.InitializeAsync( Configuration.BingMapsApiKey, BingMapType.Aerial );

        temp.Should().Be( true );
        bingMaps.Metadata.Should().NotBeNull();
        bingMaps.Metadata!.PrimaryResource.Should().NotBeNull();
        bingMaps.Metadata.PrimaryResource!.ZoomMax.Should().Be( 21 );
        bingMaps.Metadata.PrimaryResource.ZoomMin.Should().Be( 1 );
        bingMaps.Metadata.PrimaryResource.ImageHeight.Should().Be( 256 );
        bingMaps.Metadata.PrimaryResource.ImageWidth.Should().Be( 256 );
    }

    [ Theory ]
    [ InlineData( 1, 0, 0 ) ]
    [ InlineData( 2, 0, 0 ) ]
    [InlineData(15, 27, 48)]
    public async void GetBingTile( int scale, int xTile, int yTile )
    {
        var bingMaps = J4JDeusEx.ServiceProvider.GetRequiredService<BingMapProjection>();
        var initialized = await bingMaps.InitializeAsync( Configuration.BingMapsApiKey, BingMapType.Aerial );

        initialized.Should().Be( true );

        bingMaps.Scale = scale;

        var stream = await bingMaps.GetTileImageAsync( new TileCoordinates( xTile, yTile ) );

        stream.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1, 0, 0, "0")]
    [InlineData(2, 0,0,"0")]
    [InlineData(2, 1, 0, "1")]
    [InlineData(2, 0, 1, "2")]
    [InlineData(2, 1, 1, "3")]
    [InlineData(3, 0, 0, "00")]
    [InlineData(3, 1, 0, "01")]
    [InlineData(3, 2, 0, "10")]
    [InlineData(3, 3, 0, "11")]
    [InlineData(3, 0, 1, "02")]
    [InlineData(3, 1, 1, "03")]
    [InlineData(3, 2, 1, "12")]
    [InlineData(3, 3, 1, "13")]
    [InlineData(3, 0, 2, "20")]
    [InlineData(3, 1, 2, "21")]
    [InlineData(3, 2, 2, "30")]
    [InlineData(3, 3, 2, "31")]
    [InlineData(3, 0, 3, "22")]
    [InlineData(3, 1, 3, "23")]
    [InlineData(3, 2, 3, "32")]
    [InlineData(3, 3, 3, "33")]
    [InlineData(4, 0, 0, "000")]
    [InlineData(4, 3, 2, "031")]
    [InlineData(4, 4, 6, "320")]
    public async void TestQuadKeys( int scale, int xTile, int yTile, string quadKey )
    {
        var bingMaps = J4JDeusEx.ServiceProvider.GetRequiredService<BingMapProjection>();
        var initialized = await bingMaps.InitializeAsync(Configuration.BingMapsApiKey, BingMapType.Aerial);

        initialized.Should().Be(true);

        bingMaps.Scale = scale;

        bingMaps.GetQuadKey( new TileCoordinates( xTile, yTile ) ).Should().Be( quadKey );
    }
}