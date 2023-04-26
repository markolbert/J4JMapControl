# J4JMapControl: Map region properties

The region displayed by the control is defined by three properties:

|Property|Type|Comments|
|--------|----|--------|
|`Center`|`string`|must be a parseable latitude/longitude string; see below|
|`Heading`|`double`|can be any value, but is adjusted to be within the range 0 to 360|
|`MapScale`|`double`|converted to an integer internally. The value is adjusted internally if it falls outside the projection's supported range.|

Latitude/longitude strings must be in the following format:

- latitude numeric value (`double`)
- latitude direction (`char`; optional - valid values are **N** or **S**, case doesn't matter)
- comma
- longitude numeric value (`double`)
- longitude direction (`char`; optional - valid values are **E** or **W**, case doesn't matter)

If the latitude direction component is not provided the parser assumes *positive* latitudes are *north* while *negative* latitudes are *south*. Similarly, if the longitude direction component is not provided the parser assumes *positive* longitudes are *east* while *negative* longitudes are *west*.

The map's heading can also be set by calling the `SetHeadingByText()` method, supplying one of the following compass rose directions: N, E, S, W, NE, SE, SW, NW, NNE, ENE, ESE, SSE, SSW, WSW, WNW, NNW. Any other values are ignored.

[return to overview](map-control.md#basic-usage)
