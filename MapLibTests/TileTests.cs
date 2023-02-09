using FluentAssertions;
using J4JMapLibrary;
using J4JMapLibrary.Viewport;
using J4JSoftware.DeusEx;
using Microsoft.Extensions.DependencyInjection;

namespace MapLibTests;

public class TileTests : TestBase
{
    [Theory]
    [InlineData("BingMaps", 1, 0, 0, 128, 256, 0, 0, 0, 1, 1)]
    [InlineData("OpenStreetMaps", 0, 0, 0, 128, 256, 0, 0, 0, 0, 0)]
    [InlineData("BingMaps", 2, 37, -122, 128, 256, 0, 0, 2, 1, 2)]
    [InlineData("BingMaps", 2, 37, -122, 128, 256, 45, 0, 1, 1, 2)]
    [InlineData("BingMaps", 2, 37, -122, 128, 512, 45, 0, 1, 1, 3)]
    [InlineData("BingMaps", 3, 37, -122, 512, 512, 45, 0, 3, 2, 6)]
    [InlineData("BingMaps", 3, 37, -122, 512, 512, 75, 0, 3, 2, 6)]
    [InlineData("BingMaps", 3, 37, 97, 512, 512, 75, 4, 3, 7, 6)]
    [InlineData("BingMaps", 3, 37, 97, 512, 512, 0, 5, 3, 7, 5)]
    public async Task BingMapsTileRegion(
        string projectionName, 
        int scale, 
        float latitude, 
        float longitude, 
        int height,
        int width, 
        float heading, 
        int minTileX, 
        int minTileY, 
        int maxTileX, 
        int maxTileY)
    {
        var result = await GetFactory().CreateMapProjection(projectionName, null);
        var projection = result.Projection;
        projection.Should().NotBeNull();
        projection!.Initialized.Should().BeTrue();

        projection.SetScale(scale);

        var test = J4JDeusEx.ServiceProvider.GetService<Viewport>();
        test.Should().NotBeNull();

        //var tiles = await test!.Projection(projection)
        //    .Center(latitude, longitude)
        //    .Height(height)
        //    .Width(width)
        //    .Heading(heading)
        //    .GetViewportTilesAsync(GetCancellationToken(500), true);

        //tiles.Should().NotBeNull();
        //tiles!.TryGetBounds(out var bounds).Should().BeTrue();

        //var testBounds = new TileBounds(
        //        new TileCoordinates(minTileX, minTileY), 
        //        new TileCoordinates(maxTileX, maxTileY));

        //bounds!.Should().Be(testBounds);

        //await foreach (var tile in tiles.GetTilesAsync(projection, GetCancellationToken(500)))
        //{
        //    tile.ImageBytes.Should().BeNegative();
        //}
    }
}