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
            var projection = await factory.CreateMapProjection( projectionName ) as IFixedTileProjection;
            projection.Should().NotBeNull();
            projection!.Initialized.Should().BeTrue();

            if( projection is not BingMapsProjection bingMaps )
                continue;

            bingMaps.Metadata.Should().NotBeNull();
            bingMaps.Metadata!.PrimaryResource.Should().NotBeNull();
            bingMaps.Metadata.PrimaryResource!.ZoomMax.Should().Be( 21 );
            bingMaps.Metadata.PrimaryResource.ZoomMin.Should().Be( 1 );
            bingMaps.Metadata.PrimaryResource.ImageHeight.Should().Be( 256 );
            bingMaps.Metadata.PrimaryResource.ImageWidth.Should().Be( 256 );
        }
    }

    [ Theory ]
    [ InlineData( 0, true ) ]
    [ InlineData( 1, false ) ]
    public async Task BingApiKeyLatency( int maxLatency, bool result )
    {
        var factory = GetFactory();

        var options = new MapProjectionOptions( Authenticate: false );

        var projection = await factory.CreateMapProjection( "BingMaps", options ) as BingMapsProjection;
        projection.Should().NotBeNull();

        projection!.MaxRequestLatency = maxLatency;

        Configuration.TryGetCredential( "BingMaps", out var credentials ).Should().BeTrue();
        await projection.Authenticate( credentials! );

        projection.Initialized.Should().Be( result );
    }

    [ Theory ]
    [InlineData(0, 0, 0, "0")]
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
        var projection = await GetFactory().CreateMapProjection( "BingMaps" ) as IFixedTileProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var mapTile = await FixedMapTile.CreateAsync( projection, xTile, yTile, GetCancellationToken( 500 ) );
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
        var projection = await GetFactory().CreateMapProjection("OpenStreetMaps") as IFixedTileProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var mapTile = await FixedMapTile.CreateAsync(projection, xTile, yTile, GetCancellationToken(500));
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
        var projection = await GetFactory().CreateMapProjection("OpenTopoMaps") as IFixedTileProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var mapTile = await FixedMapTile.CreateAsync(projection, xTile, yTile, GetCancellationToken(500));
        mapTile.Should().NotBeNull();
        mapTile.QuadKey.Should().Be(quadKey);
    }
}