# J4JMapLibrary: Usage

- [determining which map service you want to use and setting up an account with it, if required](services.md)
- [creating a `Projection` instance for the service](creating-a-projection.md)
- [authenticating the projection](authentication.md)
- [using `ProjectionFactory` to get a `Projection` instance](factory.md)
- how imagery is returned
  - [The MapRegion Object](map-region.md)
  - [The MapTile Object](maptile.md)
  - [loading image data into a MapTile object](projection.md#retrieving-imagery)
- Other topics
  - [details on the `Projection` classes](projection.md)
  - [writing your own projection class](custom-projection.md)
  - [a note on the test routines](test.md)

Using the library involves the following steps:

- creating an instance of the projection class supporting the mapping service you want to use
- authenticating the projection instance
- defining a `MapRegion` (which describes the display area you're interested in and how it relates to the underlying map service's map at a given scale)
- loading the `MapRegion` with image data, or defining a `MapTile` within the `MapRegion` and loading *it* with image data. This is done using one of the [image retrieval methods](projection.md#retrieving-imagery) defined for the projection.
