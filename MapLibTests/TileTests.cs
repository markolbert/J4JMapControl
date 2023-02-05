using System.Collections;
using System.Runtime.CompilerServices;
using FluentAssertions;
using J4JMapLibrary;
using J4JMapLibrary.Viewport;
using J4JSoftware.DeusEx;
using Microsoft.Extensions.DependencyInjection;

namespace MapLibTests;

public class TileTests : TestBase
{
    [Theory]
    [MemberData(nameof(TileDataSource.GetTestData), "BingMaps", MemberType = typeof(TileDataSource))]
    public async Task BingMapsTileRegion( TileDataSource.Data item )
    {
        var projection = await GetFactory().CreateMapProjection( item.ProjectionName ) as ITiledProjection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        var test = J4JDeusEx.ServiceProvider.GetService<ViewportRectangle>();
        test.Should().NotBeNull();

        var tiles = await test!.Projection( projection )
                               .Center( item.CenterLatitude, item.CenterLongitude )
                               .Height( item.Height )
                               .Width( item.Width )
                               .Heading( item.Heading )
                               .GetViewportTilesAsync( GetCancellationToken( 500 ), true );

        tiles.Should().NotBeNull();
        tiles!.TryGetBounds( out var bounds ).Should().BeTrue();
        bounds!.Should().Be( item.TileBounds );

        await foreach( var tile in tiles!.GetTilesAsync( projection, GetCancellationToken( 500 ) ) )
        {
            tile.ImageBytes.Should().BeNegative();
        }
    }
}