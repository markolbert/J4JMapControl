# J4JMapControl: Parseable Location Formats

`J4JMapControl` depends on being able to define map locations through latitude and longitude coordinates. Those are handled internally as `float` values.

However, using a `float` is not particularly helpful when interfacing with other code. That's because latitude and longitude values can be specified in a variety of ways: 10, -10, 10E, 10 East, 10East, etc. They also can be, and often are, defined together: "10N, 5S".

`J4JMapControl` addresses this by requiring latitude, longitude and combined latitude/longitude values to be specified as `string` values. It parses those strings into `float` values internally, using any alphabetic suffixes to adjust the `float` value's sign.

A valid latitude or longitude `string` is structured like this:

- an optional minus sign (-)
- a `float` value
- zero or more spaces
- an optional *directional suffix*

Directional suffixes are case-insensitive.

For latitude values, the allowable directional suffixes are:

- N
- North
- S
- South

For longitude values, the allowable directional suffixes are:

- E
- East
- W
- West

A valid combined latitude/longitude value is structured like this:

- a valid latitude string
- zero or more spaces
- a comma
- zero or more spaces
- a valid longitude string

[return to overview](map-control#basic-usage)
