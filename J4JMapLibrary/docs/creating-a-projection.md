# J4JMapLibrary: Creating a Projection Instance

To simplify obtaining a `Projection` instance I incorporated a factory method in the library. However, you can also obtain an instance by instantiating the projection yourself. These first few sections describe how to do that. To see how to use the factory approach, [jump to this section](factory.md).

Each service has its own unique projection class:

|Service|Projection Class|Constructor Parameters|
|-------|----------------|----------------------|
|Bing Maps|`BingMapsProjection`|`ILoggerFactory?` loggerFactory = null|
|Google Maps|`GoogleMapsProjection`|`ILoggerFactory?` loggerFactory = null|
|Open Street Maps|`OpenStreetMapsProjection`|`ILoggerFactory?` loggerFactory = null|
|Open Topo Maps|`OpenTopoMapsProjection`|`ILoggerFactory?` loggerFactory = null|

`ILoggerFactory` is simply an instance you obtain from Microsoft's logging service. Consult their documentation for details. If you do not specify an `ILoggerFactory` instance you will not get logging events.

Three of the supported projections also support caching. `GoogleMapsProjection` does not, because *the Google Maps Static API terms of use forbid caching the retrieved imagery.* For details on enabling caching, [consult the caching documentation](caching.md).

[return to creating a projection](creating-a-projection.md)

[return to using the map factory](factory.md)
