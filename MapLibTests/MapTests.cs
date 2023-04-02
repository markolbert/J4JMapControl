using FluentAssertions;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;

namespace MapLibTests;

public class MapTests : TestBase
{
    [ Fact ]
    public async Task ValidApiKey()
    {
        foreach( var projectionName in ProjectionNames )
        {
            var projection = await CreateProjection( projectionName );
            projection.Should().NotBeNull();
            projection!.Initialized.Should().BeTrue();

            if( projection is not BingMapsProjection bingMaps )
                continue;

            bingMaps.Metadata!.PrimaryResource.Should().NotBeNull();
            bingMaps.Metadata.PrimaryResource!.ZoomMax.Should().Be( 21 );
            bingMaps.Metadata.PrimaryResource.ZoomMin.Should().Be( 1 );
            bingMaps.Metadata.PrimaryResource.ImageHeight.Should().Be( 256 );
            bingMaps.Metadata.PrimaryResource.ImageWidth.Should().Be( 256 );
        }
    }

    [ Theory ]
    [ InlineData( 500, true ) ]
    [ InlineData( 0, false ) ]
    [ InlineData( 1, false ) ]
    public async Task BingApiKeyLatency( int maxLatency, bool testResult )
    {
        var credentials = GetCredentials( "BingMaps" ) as BingCredentials;
        credentials.Should().NotBeNull();
        
        var projection = await CreateProjection( "BingMaps", null, credentials ) as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.MaxRequestLatency = maxLatency;

        if( maxLatency is > 0 and < 10 )
        {
            await Assert.ThrowsAsync<TimeoutException>( async () => await projection.SetCredentialsAsync( credentials! ) );
        }
        else
        {
            var initialized = await projection.SetCredentialsAsync( credentials! );
            initialized.Should().Be( testResult );
        }
    }

    [ Theory ]
    [InlineData(0, 0, 0, "0")] // scale can't be set to zero, so it gets overridden to 1
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
        var projection = await CreateProjection("BingMaps") as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var region = new MapRegion( projection, Logger )
                    .Scale( scale )
                    .Update();

        var mapTile = new MapTile(region, yTile).SetXAbsolute(xTile);
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
        var projection = await CreateProjection("OpenStreetMaps") as OpenStreetMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var region = new MapRegion(projection, Logger)
                    .Scale(scale)
                    .Update();

        var mapTile = new MapTile(region, yTile).SetXAbsolute(xTile);
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
        var projection = await CreateProjection("OpenTopoMaps") as OpenTopoMapsProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var region = new MapRegion(projection, Logger)
                    .Scale(scale)
                    .Update();

        var mapTile = new MapTile(region, yTile).SetXAbsolute(xTile);
        mapTile.QuadKey.Should().Be(quadKey);
    }
}