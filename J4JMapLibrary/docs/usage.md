# J4JMapLibrary: Usage

## Overview

Using the library involves the following workflow:

- [determining which map service you want to use and setting up an account with it, if required](services.md)
- [creating a `Projection` instance for the service](creating-a-projection.md)
- [authenticating the projection](authentication.md)
- [using `ProjectionFactory` to get a `Projection` instance](factory.md)
- how imagery is returned
  - [The MapRegion Object](map-region.md)
  - [The MapTile Object](maptile.md)
    - [loading image data into a MapTile object](#retrieving-imagery)

## Retrieving Imagery

There are three methods defined for retrieving map imagery from a `Projection`. All of them are asynchronous, because they deal with web-based services:

```csharp
Task<MapTile> GetMapTileByProjectionCoordinatesAsync( 
    int x, 
    int y, 
    int scale, 
    CancellationToken ctx = default );

Task<MapTile> GetMapTileByRegionCoordinatesAsync( 
    int x, 
    int y, 
    int scale, 
    CancellationToken ctx = default );

Task<bool> LoadRegionAsync(
    MapRegion.MapRegion region,
    CancellationToken ctx = default
);
```

The first two return a specific `MapTile`, which contains the image data. The third one loads a `MapRegion` with image data.

[return to top](#overview)
