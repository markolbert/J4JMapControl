# J4JMapLibrary: Usage

## Overview

Using the library involves the following workflow:

- [determining which map service you want to use and setting up an account with it, if required](#choosing-a-map-service)
- [creating a `Projection` instance for the service](#creating-a-projection-instance)
- [authenticating the projection](#authentication)
- [using `ProjectionFactory` to get a `Projection` instance](#using-the-projection-factory)
- call the `Projection` instance's retrieval methods to obtain map imagery

## Choosing a Map Service

The library currently supports the following map services:

- [Bing Maps](https://www.bingmapsportal.com/)
- [Google Maps](https://developers.google.com/maps/documentation/maps-static/overview)
- [Open Street Maps](https://wiki.openstreetmap.org/wiki/Software_libraries)
- [Open Topo Maps](https://wiki.openstreetmap.org/wiki/OpenTopoMap)

Please note that the links for *Open Street Maps* and *Open Topo Maps* may not be the best place to start learning about what those services have to offer, their limitations, etc. I found I had to do some spelunking to learn how to use them. OTOH, they're more or less open source.

Here's a brief comparison of the services as I understand them:

|Service|Projection Class|Results Can Be Cached?|Requires Account?|Payment Required?|Usage Limits|
|:-----:|:--------------:|:--------------------:|:---------------:|:---------------:|:----------:|
|Bing Maps|`BingMapsProjection`|Yes|Yes|Yes, if monthly quota exceeded|No, but can trigger financial obligation|
|Google Maps|`GoogleMapsProjection`|No|Yes|Yes, if monthly quota exceeded|No, but can trigger financial obligation|
|Open Street Maps|`OpenStreetMapsProjection`|Yes|No|No, but donations encouraged|Yes|
|Open Topo Maps|`OpenTopoMapsProjection`|Yes|No|No, but donations encouraged|Yes|

*Open Street Maps* and *Open Topo Maps* technically don't require credentials to use them. But each requires you identify your app. This is done by supplying a (hopefully unique) *user agent* string. You can lose your ability to access the services if you use someone else's user agent value, or fail to provide one. Consult the services' documentation for how to choose the required user agent value.

Bing Maps and Google Maps offer the most diverse range of "views" (e.g., road, aerial).

*Google Maps explicitly forbids caching what you retrieve. The library is designed to respect that.*

Consult the service's online documentation on how to set up an account and obtain credentials, if required.

[return to top](#overview)

## Creating a Projection Instance

To simplify obtaining a `Projection` instance I incorporated a factory method in the library. However, you can also obtain an instance by instantiating the projection yourself. These first few sections describe how to do that. To see how to use the factory approach, [jump to this section](#using-the-projection-factory).

Each service has its own unique projection class:

|Service|Projection Class|Constructor Parameters|
|-------|----------------|----------------------|
|Bing Maps|`BingMapsProjection`|`ILoggerFactory?` loggerFactory = null<br>`ITileCache?` tileCache = null|
|Google Maps|`GoogleMapsProjection`|`ILoggerFactory?` loggerFactory = null|
|Open Street Maps|`OpenStreetMapsProjection`|`ILoggerFactory?` loggerFactory = null<br>`ITileCache?` tileCache = null|
|Open Topo Maps|`OpenTopoMapsProjection`|`ILoggerFactory?` loggerFactory = null<br>`ITileCache?` tileCache = null|

Three of the projections accept optional instances of `ITileCache` and `ILoggerFactory`. The `GoogleMapsProjection` constructor, however, only accepts an optional `ILoggerFactory`. That's because *the Google Maps Static API terms of use forbid caching the retrieved imagery.*

`ILoggerFactory` is simply an instance you obtain from Microsoft's logging service. Consult their documentation for details.

`ITileCache` is an instance of one or more classes that implement caching. The interface looks like this:

```csharp
public interface ITileCache : IEnumerable<CachedEntry>
{
    ITileCache? ParentCache { get; }
    CacheStats Stats { get; }

    void Clear();
    void PurgeExpired();

    Task<bool> LoadImageAsync( MapTile mapTile, CancellationToken ctx = default );
    Task<bool> AddEntryAsync( MapTile mapTile, CancellationToken ctx = default );
}
```

The caching system is designed to work in a tiered fashion, with each *level* of caching optinally calling upon a parent cache if it itself cannot satisfy the request for a map image. 

The library comes with two caching classes: `MemoryCache` and `FileSystemCache`. What they do is pretty self-explanatory. Refer to the [caching documentation](caching.md) for more details on how to design your own cache.

You'd typically set up a multi-level caching system like this:

```csharp
var tileFileCache = new FileSystemCache(LoggerFactory)
    {
        CacheDirectory = FileSystemCachePath,
        MaxBytes = FileSystemCacheSize <= 0 ? DefaultFileSystemCacheSize : FileSystemCacheSize,
        MaxEntries = FileSystemCacheEntries <= 0 ? DefaultFileSystemCacheEntries : FileSystemCacheEntries,
        RetentionPeriod = fileRetention
    };

if (!TimeSpan.TryParse(MemoryCacheRetention, out var memRetention))
    memRetention = TimeSpan.FromHours(1);

var tileMemCache = new MemoryCache(LoggerFactory)
    {
        MaxBytes = MemoryCacheSize <= 0 ? DefaultMemoryCacheSize : MemoryCacheSize,
        MaxEntries = MemoryCacheEntries <= 0 ? DefaultMemoryCacheEntries : MemoryCacheEntries,
        ParentCache = tileFileCache,
        RetentionPeriod = memRetention
    };
```

[return to top](#overview)

## Authentication

After creating a projection instance you call one of its authentication methods to enable using it:

```csharp
public bool SetCredentials( TAuth credentials )

public async Task<bool> SetCredentialsAsync( 
    TAuth credentials, 
    CancellationToken ctx = default )
```

Each service uses a different `TAuth` class:

|Service|TAuth Class|Details|
|-------|-----------|-------|
|Bing Maps|`BingCredentials`|`ApiKey` property holds API key|
|Google Maps|`GoogleCredentials`|`ApiKey` property holds API key<br>`SignatureSecret` property holds your signature secret|
|Open Street Maps|`OpenStreetCredentials`|`UserAgent` property holds user agent string|
|Open Topo Maps|`OpenTopoCredentials`|`UserAgent` property holds user agent string|

The authentication methods return true if they succeed, false if they don't.

The only service where authentication might fail is Bing Maps, because Bing Maps requires interaction with the web to complete authentication and acquire various pieces of information required to access the service. That can fail for many reasons, including inability to access the web.

[return to top](#overview)

## Using the Projection Factory

