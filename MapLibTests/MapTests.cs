using FluentAssertions;
using J4JMapLibrary;

namespace MapLibTests;

public class MapTests : TestBase
{
    [ Fact ]
    public async void ValidApiKey()
    {
        var factory = GetFactory();

        foreach( var projectionName in factory.ProjectionNames )
        {
            var projection = await factory.CreateMapProjection(projectionName) as ITiledProjection;
            projection.Should().NotBeNull();
            projection!.Initialized.Should().BeTrue();

            if( projection is not BingMapsProjection bingMaps )
                continue;

            bingMaps.Metadata.Should().NotBeNull();
            bingMaps.Metadata!.PrimaryResource.Should().NotBeNull();
            bingMaps.Metadata.PrimaryResource!.ZoomMax.Should().Be(21);
            bingMaps.Metadata.PrimaryResource.ZoomMin.Should().Be(1);
            bingMaps.Metadata.PrimaryResource.ImageHeight.Should().Be(256);
            bingMaps.Metadata.PrimaryResource.ImageWidth.Should().Be(256);
        }
    }

    [ Theory ]
    [ InlineData( 0, true ) ]
    [ InlineData( 1, false ) ]
    public async void BingApiKeyLatency( int maxLatency, bool result )
    {
        var factory = GetFactory();

        var options = new MapProjectionOptions(Authenticate: false);

        var projection = await factory.CreateMapProjection( "BingMaps", options ) as BingMapsProjection;
        projection.Should().NotBeNull();

        projection!.MaxRequestLatency = maxLatency;

        Configuration.TryGetCredential( "BingMaps", out var credentials ).Should().BeTrue();
        await projection.Authenticate(credentials!);

        projection.Initialized.Should().Be( result );
    }

    [Theory ]
    [ InlineData( 1, 0, 0, "0" ) ]
    [ InlineData( 2, 0, 0, "00" ) ]
    [ InlineData( 2, 1, 0, "01" ) ]
    [ InlineData( 2, 0, 1, "02" ) ]
    [ InlineData( 2, 1, 1, "03" ) ]
    [ InlineData( 3, 0, 0, "000" ) ]
    [ InlineData( 3, 1, 0, "001" ) ]
    [ InlineData( 3, 2, 0, "010" ) ]
    [ InlineData( 3, 3, 0, "011" ) ]
    [ InlineData( 3, 0, 1, "002" ) ]
    [ InlineData( 3, 1, 1, "003" ) ]
    [ InlineData( 3, 2, 1, "012" ) ]
    [ InlineData( 3, 3, 1, "013" ) ]
    [ InlineData( 3, 0, 2, "020" ) ]
    [ InlineData( 3, 1, 2, "021" ) ]
    [ InlineData( 3, 2, 2, "030" ) ]
    [ InlineData( 3, 3, 2, "031" ) ]
    [ InlineData( 3, 0, 3, "022" ) ]
    [ InlineData( 3, 1, 3, "023" ) ]
    [ InlineData( 3, 2, 3, "032" ) ]
    [ InlineData( 3, 3, 3, "033" ) ]
    public async void CreateQuadKeys( int scale, int xTile, int yTile, string quadKey )
    {
        var factory = GetFactory();

        foreach( var projectionName in factory.ProjectionNames )
        {
            var projection = await GetFactory().CreateMapProjection( projectionName ) as ITiledProjection;
            projection.Should().NotBeNull();
            projection!.Initialized.Should().BeTrue();

            projection.Scale = scale;

            var mapTile = await MapTile.CreateAsync(projection, xTile, yTile);
            mapTile.Should().NotBeNull();
            mapTile.QuadKey.Should().Be( quadKey );
        }
    }
}