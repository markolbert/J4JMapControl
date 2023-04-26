# J4JMapControl: Caching properties

Many, but not all, projections support caching the retrieved imagery. There are a variety of properties available to define how caching works:

|Property|Type|Default|Comments|
|--------|----|-------|--------|
|`UseMemoryCache`|`bool`|`true`|controls whether or not in-memory caching is used when possible|
|`MemoryCacheEntries`|`int`|`DefaultMemoryCacheEntries`|defines how many entries the in-memory cache will retain before it begins purging expired ones|
|`MemoryCacheRetention`|`string`|`string.Empty`|defines how long entries are retained in the in-memory cache.|
|`MemoryCacheSize`|`int`|`DefaultMemoryCacheSize`|defines the maximum size, in bytes, the in-memory cache can be before it begins purging expired entries|
|`FileSystemCachePath`|`string`|`string.Empty`|sets the folder where the `FileSystemCache` stores its files. If undefined, empty or an invalid location file system caching is disabled.|
|`FileSystemCacheEntries`|`int`|`DefaultFileSystemCacheEntries`|defines how many files the file system cache will retain before it begins purging expired ones|
|`FileSystemCacheRetention`|`string`|`string.Empty`|defines how long entries are retained in the file system cache.|
|`FileSystemCacheSize`|`int`|`DefaultFileSystemCacheSize`|defines the maximum size, in bytes, the file system cache can store in files be before it begins purging expired entries|

[return to overview](map-control.md#basic-usage)
