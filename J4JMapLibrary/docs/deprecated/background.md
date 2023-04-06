# J4JMapLibrary: Background

## Types of Map Services

The online map services I'm familiar with fall into two categories, **tiled** and **static**. The difference relates to how the map imagery is requested and returned.

- [Tiled Services](#tiled-services)
- [Static Services](#static-services)

### Tiled Services

**Tiled** services require you to specify the *tile coordinates* of a tile you want. This is sent to the service via a REST API and, provided you've requested a valid tile, the binary image data for that tile is returned. The reason these are called tiled services is because they break up the map of the world into pre-rendered images that do not require any processing in order to generate and return; retrieving a tile is merely a kind of lookup operation.

All the tiled services currently supported by the library use what is essentially a square Mercator projection of the world. Because the north and south poles of a Mercator projection are located at infinity the projection spans "only" 85.051 degrees South to 85.051 degrees North (approximately). The projection covers the entire range of longitude (i.e., 180 degrees West to 180 degrees East).

Each tiled service renders its tiles in a fixed size, generally 256 pixels wide by 256 pixels high. The number of tiles needed to cover the map of the world depends on a **scale** or **zoom factor**. Increasing the scale by 1 doubles the number of tiles both horizontally and vertically, increasing the total number of tiles by 4.

Tiles are identified by horizontal and vertical coordinates, ranging from *0* (upper left corner) to *2 ^ scale - 1* (lower right corner):

|Scale|Horizontal Range|Vertical Range|Number of Tiles|
|:---:|:--------------:|:------------:|:-------------:|
|0|0|0|1|
|1|0 - 1|0 - 1|4|
|2|0 - 3|0 - 3|16|
|...|
|15|0 - 32,767|0 - 32,767|1,073,741,824|

Each tiled service has an upper limit on the scale factor it supports. Some also support higher zoom factors in some parts of the world than in others.

While most tiled services start with a scale level of 0, there are exceptions. In particular *Bing Maps* has a minimum scale level of 1. Consequently, the smallest scale map of the world in Bing Maps consists of 4 tiles. Tiled services starting at a scale level of 0 cover the entire world at the scale with one tile.

How you specify a particular tile depends on the service. Open Street Maps and Open Topo Maps use a URL which separately specifies the scale/zoom and the horizontal and vertical tile coordinates:

```http
https://tile.openstreetmap.org/{zoom}/{x}/{y}.png
```

However, Bing Maps identifies tiles by way of a *quadkey*. This is a mathematical construct derived from the scale/zoom, horizontal coordinate and vertical coordinate.

You can learn more about how Bing Maps is structured from their excellent article on the [Bing Maps Tile System](https://learn.microsoft.com/en-us/bingmaps/articles/bing-maps-tile-system). The article also contains a brief description of the mathematics involved with Mercator projections, as well as sample code (e.g., for determining quadkeys).

I haven't provided an example URL for Bing Maps because the URL for retrieving tiles must itself be retrieved from a meta-service and is thus subject to change depending upon exactly what kind of map you want (and possibly just so Microsoft can limit unauthorized use of their map service).

### Static Services

In contrast to tiled services, **static services** work more along the lines you might expect: you specify an area of the world you want a map image for, request it, and get the image data back.

The region is defined by specifying a rectangle centered on a chosen point on the Earth's surface (i.e., by way of specified latitude/longitude coordinates). As with tiled services there's also the concept of a scale or zoom factor, with higher scale values causing the rectangle to cover smaller (more focused) areas of the Earth.

Note that this approach assumes all rectangular areas are "normalized", meaning the upper and lower edges follow  lines of latitude while the right and left edges follow lines of longitude. Put another way, the heading of the rectangle -- how it's rotated relative to the two dimensional Mercator projection it is pulling from -- is always due north.

Currently, the only static service supported by the library is Google Maps. Be aware that the terms of use for Google Maps forbid caching the image data you retrieve from the service (the library does not allow Google Maps images to be cached, although it does support caching for tiled services).
