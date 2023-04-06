# J4JMapLibrary: Projection

## Overview

- [Interface](#interface)
- [Meta Information](#meta-information)
- [Map Scale](#map-scale)
- [Authentication](#authentication)
- [Retrieving Imagery](#retrieving-imagery)

`Projection` and its subclasses define how the library interfaces with various mapping services. They handle the processes of authentication and image retrieval, and provide information about a particular service's capabilities.

There is a subclass for each mapping service. They derive from two general-purpose subclasses, one for *static* map services which only ever return single images (e.g., Google Maps), and one for *tiled* map services, which return imagery organized around multiple tiles (e.g., Bing Maps). `StaticProjection` does not support caching. Projection classes derived from `TiledProjection` do.

Here is the class hierarchy with their corresponding named interfaces:

- `Projection` (`IProjection`)
  - `StaticProjection` (`IStaticProjection`)
    - `GoogleMapsProjection`
  - `TiledProjection` (`ITiledProjection`)
    - `BingMapsProjection`
    - `OpenStreetMapsProjection`
    - `OpenTopoMapsProjection`

## Interface

Here is the `IProjection` interface:

```csharp
    event EventHandler<bool>? LoadComplete;

    string Name { get; }
    string Copyright { get; }
    Uri? CopyrightUri { get; }
    bool Initialized { get; }

    float MaxLatitude { get; }
    float MinLatitude { get; }
    MinMax<float> LatitudeRange { get; }

    float MaxLongitude { get; }
    float MinLongitude { get; }
    MinMax<float> LongitudeRange { get; }

    int MaxRequestLatency { get; set; }
    int TileHeightWidth { get; }
    string ImageFileExtension { get; }

    int MinScale { get; }
    int MaxScale { get; }
    MinMax<int> ScaleRange { get; }
    int GetHeightWidth( int scale );
    MinMax<float> GetXYRange( int scale );
    MinMax<int> GetTileRange( int scale );
    int GetNumTiles( int scale );

    bool SetCredentials( object credentials );
    
    Task<bool> SetCredentialsAsync( 
        object credentials, 
        CancellationToken ctx = default );

    byte[]? GetImage( MapTile mapTile );
    Task<byte[]?> GetImageAsync( MapTile mapTile, CancellationToken ctx = default );

    Task<MapTile> GetMapTileWraparoundAsync( 
        int xTile, 
        int yTile, 
        int scale, 
        CancellationToken ctx = default );

    Task<MapTile> GetMapTileAbsoluteAsync( 
        int xTile, 
        int yTile, 
        int scale, 
        CancellationToken ctx = default );

    Task<bool> LoadImageAsync( MapTile mapTile, CancellationToken ctx = default );

    Task<bool> LoadRegionAsync(
        MapRegion.MapRegion region,
        CancellationToken ctx = default
    );
```

[return to top](#overview)

[return to usage table of contents](usage.md)

## Meta Information

`Projection` defines a number of properties which simply describe a mapping service:

|Property|Type|Description|
|--------|----|-----------|
|`Name`|`string`|the unique name of the projection/mapping service|
|`Copyright`|`string`|holds the copyright notice most services require you to display a copyright notice when you display their imagery. Set when the projection is authenticated.|
|`CopyrightUri`|`Uri?`|holds the copyright Uri some services require you to display when you display their imagery. Set when the projection is authenticated.|
|`Initialized`|`bool`|`true` after the projection is initialized and authenticated, `false` otherwise|
|`MaxLatitude`|float|the maximum latitude supported by the projection|
|`MinLatitude`|float|the minimum latitude supported by the projection|
|`LatitudeRange`|`MinMax<float>`|used to conform latitude values to the allowable range of latitude values|
|`MaxLongitude`|float|the maximum longitude supported by the projection|
|`MinLongitude`|float|the minimum longitude supported by the projection|
|`LongitudeRange`|`MinMax<float>`|used to conform longitude values to the allowable range of longitude values|
|`MaxRequestLatency`|`int`|the maximum number of milliseconds a web request will wait to complete. Negative values mean 'wait forever'.|
|`TileHeightWidth`|`int`|the height and width of the map service's tiles. Generally 256. Even *static* projections, like Google Maps, which do not return tiles use tiles internally.|
|`ImageFileExtension`|`string`|defines the nature of the image data returned by a mapping service. Used when creating cached files by `FileSystemCache`.|

`Name` is used by other parts of the library to identify a particular projection class. The value of `Name` should be unique within an application (and, ideally, throughout the world so that any custom projection classes you write won't collide with someone else's).

[return to top](#overview)

[return to usage table of contents](usage.md)

## Map Scale

While a `Projection` defines the interaction with a mapping service, it does not define the details of the map service's underlying map. That's because the underlying map changes depending upon the *scale* at which it is viewed or used. `Projection` defines a number of properties and methods to work with map scaling.

|Property|Type|Description|
|--------|----|-----------|
|`MinScale`|`int`|the minimum scale factor supported by the map service. Usually, but not always, 0 (Bing Maps' minimum is 1)|
|`MaxScale`|`int`|the maximum scale factor supported by the map service|
|`ScaleRange`|`MinMax<int>`|used to conform map scale values to the allowable range of map scale values|

|Method|Return Value|Arguments|Description|
|------|------------|---------|-----------|
|`GetHeightWidth`|`int`|`int` scale|gets the height and width of the map service's underlying map at a given map scale (the underlying maps are square)|
|`GetXYRange`|`MinMax<float>`|`int` scale|gets the allowable range of X and Y Cartesian coordinates of the map service's underlying map at a given map scale|
|`GetTileRange`|`MinMax<int>`|`int` scale|gets the allowable range of horizontal/vertical *tile coordinates* of the map service's underlying map at a given map scale|
|`GetNumTiles`|`int`|`int` scale|gets the total number of tiles in the map service's underlying map at a given map scale|

[return to top](#overview)

[return to usage table of contents](usage.md)

## Authentication

Authenticating a `Projection` relies on processing a *credentials* object. There are two methods for doing so:

```csharp
bool SetCredentials( object credentials );

Task<bool> SetCredentialsAsync( 
    object credentials, 
    CancellationToken ctx = default );
```

The *credentials* object is projection-specific. If you supply the wrong kind of object authentication will fail. Here are the credentials objects for each of the supported projections:

|Projection Type|Credentials Type|
|---------------|----------------|
|`BingMapsProjection`|`BingCredentials`|
|`GoogleMapsProjection`|`GoogleCredentials`|
|`OpenStreetMapsProjection`|`OpenStreetCredentials`|
|`OpenTopoMapsProjection`|`OpenTopoCredentials`|

The structure of a credential type reflects the information needed to authenticate with its mapping service:

|Credentials Type|Property|Property Type|Comments|
|----------------|--------|-------------|-----------|
|`BingCredentials`|`ApiKey`|`string`||
|`GoogleCredentials`|`ApiKey`|`string`||
||`SignatureSecret`|`string`|used to encrypt each web request|
|`OpenStreetCredentials`|`UserAgent`|chosen by you, but must uniquely identify your application. Consult the online documentation for details|
|`OpenTopoCredentials`|`UserAgent`|chosen by you, but must uniquely identify your application. Consult the online documentation for details|

[return to top](#overview)

[return to usage table of contents](usage.md)

## Retrieving Imagery

There are five methods defined for retrieving map imagery from a `Projection`. All of them are asynchronous, because they deal with web-based services:

```csharp
Task<byte[]?> GetImageAsync( MapTile mapTile, CancellationToken ctx = default );

Task<MapTile> GetMapTileWraparoundAsync( 
    int xTile, 
    int yTile, 
    int scale, 
    CancellationToken ctx = default );

Task<MapTile> GetMapTileAbsoluteAsync( 
    int xTile, 
    int yTile, 
    int scale, 
    CancellationToken ctx = default );

Task<bool> LoadImageAsync( MapTile mapTile, CancellationToken ctx = default );

Task<bool> LoadRegionAsync(
    MapRegion.MapRegion region,
    CancellationToken ctx = default
);
```

They differ in whether they simply return `byte[]` data or load an object (i.e., `MapTile`, `MapRegion`) with `byte[]` data:

|Method|Arguments|Comments|
|------|---------|--------|
|`GetImageAsync`|`MapTile` mapTile|gets the image data associated with a map service tile described by `mapTile`|
||`CancellationToken` ctx||
|`GetMapTileWraparoundAsync`|`int` xTile|creates and loads a `MapTile` based on the supplied `scale`, `xTile` and `yTile` values. Wraparound is supported.|
||`int` yTile||
||`int` scale|the map scale factor to use in defining the `MapTile`|
||`CancellationToken` ctx||
|`GetMapTileAbsoluteAsync`|`int` xTile|creates and loads a `MapTile` based on the supplied `scale`, `xTile` and `yTile` values. Wraparound is *not* supported.|
||`int` yTile||
||`int` scale|the map scale factor to use in defining the `MapTile`|
||`CancellationToken` ctx||
|`LoadImageAsync`|`MapTile` mapTile|loads image data into `mapTile`, getting it either from the cache (if one is defined) or the mapping service.|
||`CancellationToken` ctx||
|`LoadRegionAsync`|`MapRegion` region|loads image data into all the MapTiles defined for `region`, getting it either from the cache (if one is defined) or the mapping service.|
||`CancellationToken` ctx||

[return to top](#overview)

[return to usage table of contents](usage.md)
