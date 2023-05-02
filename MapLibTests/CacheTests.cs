using FluentAssertions;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.DeusEx;
using Microsoft.Extensions.DependencyInjection;

namespace MapLibTests;

public class CacheTests : TestBase
{
    // scales above 4 take over 10 seconds each
    [ Theory ]
    [ InlineData( 4, 0 ) ]
    [ InlineData( 4, 30 ) ]
    public async Task MemoryCacheCount( int scale, int maxCached )
    {
        var projection = CreateAndAuthenticateProjection( "BingMaps" ) as BingMapsProjection;
        projection.Should().NotBeNull();

        var cache = J4JDeusEx.ServiceProvider.GetService<MemoryCache>();
        cache.Should().NotBeNull();
        cache!.MaxEntries = maxCached;

        projection!.TileCaching.AddCache( cache);

        var maxTile = projection.GetTileRange( scale ).Maximum;

        for( var xTile = 0; xTile <= maxTile; xTile++ )
        {
            for( var yTile = 0; yTile <= maxTile; yTile++ )
            {
                await projection.GetMapTileWraparoundAsync( xTile, yTile, scale );

                if (maxCached > 0)
                    cache.Stats.Entries.Should().BeLessOrEqualTo(maxCached);
            }
        }
    }

    [Theory]
    [InlineData(4, 0 )]
    [InlineData(4, 30 )]
    public async Task FileSystemCacheCount( int scale, int maxCached )
    {
        var cache = J4JDeusEx.ServiceProvider.GetService<FileSystemCache>();
        cache.Should().NotBeNull();
        cache!.MaxEntries = maxCached;
        cache.CacheDirectory = Path.Combine( Environment.CurrentDirectory, "image-cache" );
        cache.IsValid.Should().BeTrue();

        foreach( var fileName in Directory.GetFiles( cache.CacheDirectory,
                                                     "*.*",
                                                     new EnumerationOptions()
                                                     {
                                                         IgnoreInaccessible = true, RecurseSubdirectories = true
                                                     } ) )
        {
            File.Delete( fileName );
        }

        var projection = CreateAndAuthenticateProjection("BingMaps") as BingMapsProjection;
        projection.Should().NotBeNull();

        projection!.TileCaching.AddCache(cache);

        var maxTile = projection.GetTileRange( scale ).Maximum;

        for (var xTile = 0; xTile <= maxTile; xTile++)
        {
            for (var yTile = 0; yTile <= maxTile; yTile++)
            {
                await projection.GetMapTileWraparoundAsync(xTile, yTile, scale);

                if( maxCached > 0 )
                    cache.Stats.Entries.Should().BeLessOrEqualTo(maxCached);
            }
        }
    }
}
