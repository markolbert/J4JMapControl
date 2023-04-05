# J4JMapLibrary: Terminology

Developing this library taught me that, as Clauswitz observed about war, when displaying maps, everything is very simple, but the simplest thing is difficult. To make it easier to understand how the library works it pays to define some commonly-used terms.

## Table of Contents

- [Map Tiles](#map-tiles)
- [Static vs Tiled Services](#static-vs-tiled-services)
- [Heading](#heading)
- [Map Scale](#map-scale)
- [Bounding Box](#bounding-box)
- [Wraparound](#wraparound)
- [Coordinate Systems](#coordinate-systems)
- [Projection](#projection)

## Map Tiles

All the map services I'm familiar with store their imagery in the form of **map tiles**. These are square images (of an overall square map) which, because they are pre-calculated and stored, can be served very quickly.

Just because they're pre-calculated doesn't mean they don't change. In fact, the map tiles seem to get updated surprisingly frequently, which has implications for how long you should cache them.

[return to table of contents](#table-of-contents)

## Static vs Tiled Services

Also, just because a service is based on map tiles doesn't mean that what it returns are map tiles. Of the four services I'm familiar with (and which the library was built to support), three of them return tiles but one of them does not. The "outlier" is Google Maps, which returns a single image for every map request, apparently stitching together and cropping the underlying tiles.

Because it always and only returns a single image, Google Maps is what I call a **static** service (in fact, I got that name from their own API, which is called the *Maps Static API*). The other services, which return map tiles, I call **tiled** services.

The tile services use different approaches for identifying the tile you want. Bing Maps uses something called a quadkey, which has some interesting properties and which you can learn more about [here](https://learn.microsoft.com/en-us/bingmaps/articles/bing-maps-tile-system). Open Street Maps and Open Topo Maps identify tiles by their horizontal and vertical coordinates, which start at **0, 0** in the upper left corner.

[return to table of contents](#table-of-contents)

## Heading

One of the many head-scratching problems I had to solve involves the multiple coordinate systems used at various points in the retrieval and display process. Here's a picture illustrating some of the complexity:

![multiple grids](assets/world-grids.png)

The green area is what we want to display. It's rotated because the **heading**, the angle betweeen true north and the vector perpendicular to the top of the green area, isn't zero (it's rotated slightly counter-clockwise in this example).

[return to table of contents](#table-of-contents)

## Map Scale

The map tiles are defined by the black grid superimposed on the world map. The number of tiles horizontally always equals the number of tiles vertically -- the grid is a square -- and the number of tiles in either dimension is always a power of 2: 1, 2, 4, 8, etc. The **scale** of a map is the *base-2 logarithm* of the number of tiles:

|Scale|Number of Tiles|
|:---:|:-------------:|
|0|1|
|1|2|
|2|4|
|...|...|
|15|32,768|
|18|262,144|
|...|...|

Most services have a minimum scale of **0**. However, Bing Maps' minimum scale is **1**; the smallest Bing Maps grid has four map tiles.

Each service has a different maximum scale, and some have maximums that are limited based on the area of the map, your account, etc.

[return to table of contents](#table-of-contents)

## Bounding Box

Because the area we want to display has a non-zero heading, the area of tiles we need to retrieve to display the entire region is bigger than the area we're going to display.

Calculating precisely which tiles to retrieve to cover the rotated display area seemed to me like a pain in the ass, so the library simply calculates the **bounding box** which contains the rotated display region and retrieves all the tiles within it.

[return to table of contents](#table-of-contents)

## Wraparound

An additional complication arises because, unlike the map which has left and right boundaries, dear old Earth doesn't.

It's a sphere (sort of), so the display area and bounding box need to be able to wrap around. But not so much that the wraparound covers more than one full Earth width.

[return to table of contents](#table-of-contents)

## Coordinate Systems

Just in case matters weren't complex enough already, there are multiple different coordinate systems involved with each of these geometric entities. And they are not consistent with each other!

|Coordinate System|Usage|Origin|Scale Sensitive?|Increasing Y Values Move You...|
|:---------------:|:---:|:----:|:--------------:|:-----------------------------:|
|Latitude/Longitude|Identifying locations on the underlying map|someplace in west Africa|No|Up|
|Cartesian|Identifying locations within the map grid|upper left corner|Yes|Down|
|Tiles|Identifying a map tile|upper left corner|Yes|Down|
|Display|Identifying a point on the display|upper left corner|No|Down, more or less, but the actual direction depends on the **heading**|

**Scale sensitive** means "do the same coordinate values specify the same point at different map scales?"

An awful lot of the hair-pulling that occured while developing the library involved figuring out how to translate from one of these systems to another, not to mention simply remember which one was "in play" in a particular part of the code base.

Things got bad enough that, on occasion, I confess I just plugged in numbers until things looked right, and then used the "correct" values as a clue to figuring out what mathematical transform I had to apply to derive them from another coordinate system.

[return to table of contents](#table-of-contents)

## Projection

I initially started developing an object called `Projection`, defined as "a class that knows how to retrieve imagery from a map service".

I eventually realized, however, that conceptual model was wrong. Because it ignores the fact the coordinates used to retrieve map tiles is dependent on the current map scale. I find it useful to think of a map tile's "base coordinates" -- the minimal set of factors needed to fully define a map tile -- as its center (latitude, longitude) and map scale.

In it's final (latest?) form, `Projection` is still a class that retrieves map imagery. But it's completely independent of map scale, even though it is "aware" of the allowable range of map scaling factors for a given map service.

You dont' interact with the `Projection` class directly. It's a base class for `StaticProjection` (which interacts with static services, the only one of which currently supported is Google Maps) and `TiledProjection` (which interacts with Bing Maps, Open Street Maps and Open Topo Maps).

[return to table of contents](#table-of-contents)
