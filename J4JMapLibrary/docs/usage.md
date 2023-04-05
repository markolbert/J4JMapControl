# J4JMapLibrary: Usage

## Overview

Using the library involves the following workflow:

- [determining which map service you want to use and setting up an account with it, if required](services.md)
- [creating a `Projection` instance for the service](creating-a-projection.md)
- [authenticating the projection](authentication.md)
- [using `ProjectionFactory` to get a `Projection` instance](factory.md)
- how imagery is returned
  - [The MapRegion Object](map-region.md)
  - [The MapTile Object](#the-maptile-object)
- [calling the `Projection` instance's retrieval methods to obtain map imagery](#retrieving-imagery)

## The MapTile Object

The base object used to return all image data in the library is `MapTile`. This is true even for **static** projections, like Google Maps, which only ever return one 'tile' (whose size depends on the request being made).

`MapTile` is a fairly complex object, offering a lot of meta information about the image data it contains and functionality so the rest of the library can abstract away common elements.

`MapTile`s also always exist in relation to some `MapRegion`. There are no independent `MapTile`s. That's because a `MapRegion` typically contains multiple `MapTile`s, and defines certain essential features of a `MapTile` (e.g., the projection it's drawn from, the map scale it reflects).

```csharp
MapRegion Region { get; }

bool InProjection { get; }

float Height { get; set; }
float Width { get; set; }

string FragmentId { get; }
string QuadKey { get; }
int Row { get; }
int Column { get; }

byte[]? ImageData { get; set; }
long ImageBytes { get; }
byte[]? GetImage();
Task<byte[]?> GetImageAsync( CancellationToken ctx = default );
Task<bool> LoadImageAsync( CancellationToken ctx = default );
Task<bool> LoadFromCacheAsync( ITileCache? cache, CancellationToken ctx = default );

int X { get; }
int Y { get; }
MapTile SetXRelative( int relativeX );
MapTile SetXAbsolute( int absoluteX );
MapTile SetRowColumn( int row, int column );

(int X, int Y) GetUpperLeftCartesian();
```

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
