# J4JMapLibrary

[![J4JMapLibrary](https://img.shields.io/nuget/v/J4JSoftware.J4JMapLibrary?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.J4JMapLibrary/)

A Net7 library for retrieving map images from online services. The supported services are currently:

- [Bing Maps](https://www.bingmapsportal.com/)
- [Google Maps](https://developers.google.com/maps/documentation/maps-static/overview)
- [Open Street Maps](https://wiki.openstreetmap.org/wiki/Software_libraries)
- [Open Topo Maps](https://wiki.openstreetmap.org/wiki/OpenTopoMap)

Some of these services require setting up a user account to access them. No credentials are included in the library.

The library is built with nullability enabled.

All of the services have usage rules (e.g., maximum number of downloads per day) you must adhere to. Consult the documentation for each of the services to learn more.

The change log can be found [here](changes.md).

Understanding how to use this library involves a number of terms which may not be familiar to many developers. It's worth reviewing the [notes on terminology](terminology.md) before diving into the rest of the documentation.

[How to use the library](usage.md)

[Writing a custom projection class](custom-projection.md)
