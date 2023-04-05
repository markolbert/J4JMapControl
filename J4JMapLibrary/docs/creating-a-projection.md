# J4JMapLibrary: Creating a Projection Instance

To simplify obtaining a `Projection` instance I incorporated a factory method in the library. However, you can also obtain an instance by instantiating the projection yourself. These first few sections describe how to do that. To see how to use the factory approach, [jump to this section](usage.md#using-the-projection-factory).

Each service has its own unique projection class:

|Service|Projection Class|Constructor Parameters|
|-------|----------------|----------------------|
|Bing Maps|`BingMapsProjection`|`ILoggerFactory?` loggerFactory = null<br>`ITileCache?` tileCache = null|
|Google Maps|`GoogleMapsProjection`|`ILoggerFactory?` loggerFactory = null|
|Open Street Maps|`OpenStreetMapsProjection`|`ILoggerFactory?` loggerFactory = null<br>`ITileCache?` tileCache = null|
|Open Topo Maps|`OpenTopoMapsProjection`|`ILoggerFactory?` loggerFactory = null<br>`ITileCache?` tileCache = null|

Three of the projections accept optional instances of `ITileCache` and `ILoggerFactory`. The `GoogleMapsProjection` constructor, however, only accepts an optional `ILoggerFactory`. That's because *the Google Maps Static API terms of use forbid caching the retrieved imagery.*

`ILoggerFactory` is simply an instance you obtain from Microsoft's logging service. Consult their documentation for details.

`ITileCache` is an instance of one or more classes that implement caching. For details, [consult the caching documentation](caching.md).

[return to table of contentes](usage.md#overview)
