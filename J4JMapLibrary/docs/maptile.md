# J4JMapLibrary: The MapTile Object

## Overview

- [Interface](#interface)
- [The Constructor and Contextual Properties](#the-constructor-and-contextual-properties)
- [Positioning and Sizing MapTiles](#positioning-and-sizing-maptiles)

`MapTile` is the class used to hold image data in the library. This is true even for **static** projections, like Google Maps, which only have one 'tile' (whose size depends on the request being made).

MapTiles always exist in relation to some `MapRegion`. There are no independent `MapTile`s. That's because a `MapRegion` defines certain essential features of multiple `MapTile`s. This includes the projection the `MapTile` is drawn from and the map scale it embodies.

`MapTile` is a fairly complex object, offering a lot of information about the map area it represents, in addition to the actual image data. However, it itself does not retrieve image data -- it merely holds it. There are two properties directly associated with the image data:

|Property|Type|Description|
|--------|----|-----------|
|`ImageData`|`byte[]?`|a byte array of the actual image data|
|`ImageBytes`|`long`|the number of image data bytes, or -1 if there is none|

Consult the [documentation on `Projection`](projection.md) to learn about the library's image retrieval methods.

## Interface

Here's what the `MapTile` interface looks like:

```csharp
MapRegion Region { get; }
bool InProjection { get; }

float Height { get; set; }
float Width { get; set; }

int X { get; }
int Y { get; }
MapTile SetXRelative( int relativeX );
MapTile SetXAbsolute( int absoluteX );
MapTile SetRowColumn( int row, int column );
int Row { get; }
int Column { get; }

(int X, int Y) GetUpperLeftCartesian();

string FragmentId { get; }
string QuadKey { get; }

byte[]? ImageData { get; set; }
long ImageBytes { get; }

byte[]? GetImage();
Task<byte[]?> GetImageAsync( CancellationToken ctx = default );
Task<bool> LoadImageAsync( CancellationToken ctx = default );
Task<bool> LoadFromCacheAsync( ITileCache? cache, CancellationToken ctx = default );
```

[return to overview](#overview)

[return to usage table of contents](usage.md)

## The Constructor and Contextual Properties

`Region` and `InProjection` are properties that relate to the context in which a `MapTile` exists.

`Region` is the `MapRegion` object within which the `MapTile` was created. It must be specified in the `MapTile` constructor:

```csharp
public partial class MapTile : Tile
{
    public MapTile(
        MapRegion region,
        int absoluteY
    )
        : base( region, -1, absoluteY )
    {
        //...
    }

    //...
}
```

`MapTile`s are subclasses of the `Tile` class, which itself is "unaware" of things like image data. `Tile`s are essentially just locations on a tiled grid.

The other required constructor parameter is the `MapTile`'s *vertical tile coordinate* (`absoluteY`). This is *from the perspective of the projection to which the `MapRegion` is bound*. In other words, it's the row in the projection to which the `MapTile` is related.

In this context, the row number can be any integer; it doesn't have to correspond to an actual row in the current projection and the current scale. A negative number simply means it's a row "above" any row in the actual projection. A number greater than the maximum number of tiles at the current map scale simply means the row is "below" any row in the actual projection.

The `Tile` constructor requires both a row number and a column (horizontal, *X*) number. The column number can be any value, but values below 0 or above the maximum number of tiles at the current map scale imply wraparound. When a `MapTile` is created its `X` property is set to -1.

[return to overview](#overview)

[return to usage table of contents](usage.md)

## Positioning and Sizing MapTiles

MapTiles have both a position and a size. In fact -- and this complicates using them -- MapTiles have multiple *kinds* of positions associated with them:

|Property|Description|
|--------|-----------|
|`X`|the horizontal/column position of the `MapTile` within the projection, at the current map scale|
|`Y`|the vertical/row position of the `MapTile` within the projection, at the current map scale|
|`Row`|the row number, within the MapRegion's collection of MapTiles, where the `MapTile` is stored|
|`Column`|the column number, within the MapRegion's collection of MapTiles, where the `MapTile` is stored|

Moreover, the `Y` tile coordinate (row) cannot be changed after the `MapTile` is created. That's because the projections don't wraparound vertically (i.e., the top and bottom edges of the projection are not connected like the right and left edges are).

The `X` tile coordinate (column) can be changed after a `MapTile` is created. In fact, it can be changed in two different ways so as to be able support wraparound:

|X Coordinate Setter|Argument|Description|
|-------------------|--------|-----------|
|`SetXRelative`|`int` relativeX|move the `X` coordinate right (for positive values of `relativeX`) or left (for negative values of `relativeX`), taking into account the possibility of wrapping around the projection|
|`SetXAbsolute`|`int` absoluteX|set the `X` coordinate to `absoluteX`|

Both methods also update a MapTile's `QuadKey` and `FragmentId` (both described below).

The `Row` and `Column` properties are changed by calling the `SetRowColumn()` method. It checks the proposed row and column values to ensure they are within the bounds of the array holding them within the `MapRegion` object. Exceptions are thrown if they are not.

The `Height` and `Width` properties allow the `MapTile` to be sized. They can accept any values, but they are intended to be set to positive values.

`MapTile` also contains a pair of properties that describe its position in a way required to support caching and, for Bing Maps, image retrieval:

|Property|Type|Description|
|--------|----|-----------|
|`QuadKey`|`string`|a string which uniquely define's a MapTile's position. This value is needed to retrieve iamges from the Bing Maps service. You can learn more about it in the [Bing Maps documentation](https://learn.microsoft.com/en-us/bingmaps/articles/bing-maps-tile-system).|
|`FragmentId`|`string`|a string which uniquely define's a MapTile in a way which can be incorporated into a file name for caching purposes|

Both of these properties are updated automatically whenever the `X` coordinate changes.

[return to overview](#overview)

[return to usage table of contents](usage.md)
