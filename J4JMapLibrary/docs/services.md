# J4JMapLibrary: Choosing a Map Service

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

[return to usage](usage.md)
