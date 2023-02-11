using FluentAssertions;
using J4JMapLibrary;

namespace MapLibTests;

public class MapTests : TestBase
{
    [ Fact ]
    public async Task ValidApiKey()
    {
        var factory = GetFactory();

        foreach( var projectionName in factory.ProjectionNames )
        {
            var result = await factory.CreateMapProjection( projectionName, null );
            var projection = result.Projection;
            projection.Should().NotBeNull();
            projection!.Initialized.Should().BeTrue();

            if( projection is not BingMapsProjection bingMaps )
                continue;

            var bingServer = bingMaps.MapServer as BingMapServer;
            bingServer.Should().NotBeNull();

            bingServer!.Metadata!.PrimaryResource.Should().NotBeNull();
            bingServer.Metadata.PrimaryResource!.ZoomMax.Should().Be( 21 );
            bingServer.Metadata.PrimaryResource.ZoomMin.Should().Be( 1 );
            bingServer.Metadata.PrimaryResource.ImageHeight.Should().Be( 256 );
            bingServer.Metadata.PrimaryResource.ImageWidth.Should().Be( 256 );
        }
    }

    [ Theory ]
    [ InlineData( 500, true ) ]
    [ InlineData( 0, false ) ]
    [ InlineData( 1, false ) ]
    public async Task BingApiKeyLatency( int maxLatency, bool testResult )
    {
        Configuration.TryGetCredential( "BingMaps", out var rawCredentials ).Should().BeTrue();
        rawCredentials.Should().NotBeNull();        
        
        var credentials = new BingCredentials( rawCredentials!.ApiKey, BingMapType.Aerial );

        var result = await GetFactory().CreateMapProjection( "BingMaps", null, authenticate: false );
        result.Authenticated.Should().Be( false );

        var projection = result.Projection as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.MapServer.MaxRequestLatency = maxLatency;

        if( maxLatency is > 0 and < 10 )
        {
            await Assert.ThrowsAsync<TimeoutException>( async () => await projection.AuthenticateAsync( credentials ) );
        }
        else
        {
            var initialized = await projection.AuthenticateAsync( credentials );
            initialized.Should().Be( testResult );
        }
    }

    [ Theory ]
    [InlineData(0, 0, 0, "")]
    [InlineData(1, 0, 0, "0")]
    [InlineData(1, 1, 0, "1")]
    [InlineData(1, 0, 1, "2")]
    [InlineData(1, 1, 1, "3")]
    [InlineData(2, 0, 0, "00")]
    [InlineData( 2, 1, 0, "01" )]
    [InlineData( 2, 0, 1, "02" )]
    [InlineData( 2, 1, 1, "03" )]
    [InlineData( 3, 0, 0, "000" )]
    [InlineData( 3, 1, 0, "001" )]
    [InlineData( 3, 2, 0, "010" )]
    [InlineData( 3, 3, 0, "011" )]
    [InlineData( 3, 0, 1, "002" )]
    [InlineData( 3, 1, 1, "003" )]
    [InlineData( 3, 2, 1, "012" )]
    [InlineData( 3, 3, 1, "013" )]
    [InlineData( 3, 0, 2, "020" )]
    [InlineData( 3, 1, 2, "021" )]
    [InlineData( 3, 2, 2, "030" )]
    [InlineData( 3, 3, 2, "031" )]
    [InlineData( 3, 0, 3, "022" )]
    [InlineData( 3, 1, 3, "023" )]
    [InlineData( 3, 2, 3, "032" )]
    [InlineData( 3, 3, 3, "033" )]
    public async Task BingMapsQuadKeys( int scale, int xTile, int yTile, string quadKey )
    {
        var result = await GetFactory().CreateMapProjection( "BingMaps", null );
        var projection = result.Projection as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.MapScale.Scale = scale;

        var mapTile = await TiledFragment.CreateAsync( projection, xTile, yTile, scale );
        mapTile.Should().NotBeNull();
        mapTile.QuadKey.Should().Be( quadKey );
    }

    [Theory]
    [InlineData(0, 0, 0, "0")]
    [InlineData(1, 0, 0, "00")]
    [InlineData(1, 1, 0, "10")]
    [InlineData(1, 0, 1, "20")]
    [InlineData(1, 1, 1, "30")]
    [InlineData(2, 0, 0, "000")]
    [InlineData(2, 1, 0, "010")]
    [InlineData(2, 0, 1, "020")]
    [InlineData(2, 1, 1, "030")]
    [InlineData(3, 0, 0, "0000")]
    [InlineData(3, 1, 0, "0010")]
    [InlineData(3, 2, 0, "0100")]
    [InlineData(3, 3, 0, "0110")]
    [InlineData(3, 0, 1, "0020")]
    [InlineData(3, 1, 1, "0030")]
    [InlineData(3, 2, 1, "0120")]
    [InlineData(3, 3, 1, "0130")]
    [InlineData(3, 0, 2, "0200")]
    [InlineData(3, 1, 2, "0210")]
    [InlineData(3, 2, 2, "0300")]
    [InlineData(3, 3, 2, "0310")]
    [InlineData(3, 0, 3, "0220")]
    [InlineData(3, 1, 3, "0230")]
    [InlineData(3, 2, 3, "0320")]
    [InlineData(3, 3, 3, "0330")]
    public async Task OpenStreetMapsQuadKeys(int scale, int xTile, int yTile, string quadKey)
    {
        var result = await GetFactory().CreateMapProjection( "OpenStreetMaps", null );
        var projection = result.Projection as OpenStreetMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.MapScale.Scale = scale;

        var mapTile = await TiledFragment.CreateAsync(projection, xTile, yTile, scale);
        mapTile.Should().NotBeNull();
        mapTile.QuadKey.Should().Be(quadKey);
    }

    [Theory]
    [InlineData(0, 0, 0, "0")]
    [InlineData(1, 0, 0, "00")]
    [InlineData(1, 1, 0, "10")]
    [InlineData(1, 0, 1, "20")]
    [InlineData(1, 1, 1, "30")]
    [InlineData(2, 0, 0, "000")]
    [InlineData(2, 1, 0, "010")]
    [InlineData(2, 0, 1, "020")]
    [InlineData(2, 1, 1, "030")]
    [InlineData(3, 0, 0, "0000")]
    [InlineData(3, 1, 0, "0010")]
    [InlineData(3, 2, 0, "0100")]
    [InlineData(3, 3, 0, "0110")]
    [InlineData(3, 0, 1, "0020")]
    [InlineData(3, 1, 1, "0030")]
    [InlineData(3, 2, 1, "0120")]
    [InlineData(3, 3, 1, "0130")]
    [InlineData(3, 0, 2, "0200")]
    [InlineData(3, 1, 2, "0210")]
    [InlineData(3, 2, 2, "0300")]
    [InlineData(3, 3, 2, "0310")]
    [InlineData(3, 0, 3, "0220")]
    [InlineData(3, 1, 3, "0230")]
    [InlineData(3, 2, 3, "0320")]
    [InlineData(3, 3, 3, "0330")]
    public async Task OpenTopoMapsQuadKeys(int scale, int xTile, int yTile, string quadKey)
    {
        var result = await GetFactory().CreateMapProjection( "OpenTopoMaps", null );
        var projection = result.Projection as OpenTopoMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.MapScale.Scale = scale;

        var mapTile = await TiledFragment.CreateAsync(projection, xTile, yTile, scale);
        mapTile.Should().NotBeNull();
        mapTile.QuadKey.Should().Be(quadKey);
    }
}