﻿using FluentAssertions;
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
        var cache = J4JDeusEx.ServiceProvider.GetService<MemoryCache>();
        cache.Should().NotBeNull();
        cache!.MaxEntries = maxCached;

        var result = await GetFactory().CreateMapProjection( "BingMaps", cache );
        var projection = result.Projection as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.MapScale.Scale = scale;

        var numCreated = 0;

        for( var xTile = 0; xTile <= projection.TileXRange.Maximum; xTile++ )
        {
            for( var yTile = 0; yTile <= projection.TileYRange.Maximum; yTile++ )
            {
                await TiledFragment.CreateAsync( projection, xTile, yTile );
                numCreated++;

                cache.Count.Should().Be( maxCached <= 0 || numCreated <= maxCached ? numCreated : maxCached );
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

        var result = await GetFactory().CreateMapProjection("BingMaps", cache);
        var projection = result.Projection as BingMapsProjection;
        projection.Should().NotBeNull();
        projection!.MapScale.Scale = scale;

        var numCreated = 0;

        for (var xTile = 0; xTile <= projection.TileXRange.Maximum; xTile++)
        {
            for (var yTile = 0; yTile <= projection.TileYRange.Maximum; yTile++)
            {
                await TiledFragment.CreateAsync( projection, xTile, yTile );
                numCreated++;

                cache.Count.Should().Be(maxCached <= 0 || numCreated <= maxCached ? numCreated : maxCached);
            }
        }
    }
}
