# J4JMapControl: Map region properties

The region displayed by the control is defined by three properties:

|Property|Type|Comments|
|--------|----|--------|
|`Center`|`string`|must be a [parseable combined latitude/longitude](parseable-location-formats.md) value|
|`Heading`|`double`|can be any value, but is adjusted to be within the range 0 to 360|
|`MapScale`|`double`|converted to an integer internally. The value is adjusted internally if it falls outside the projection's supported range.|

The map's heading can also be set by calling the `SetHeadingByText()` method, supplying one of the following compass rose directions: N, E, S, W, NE, SE, SW, NW, NNE, ENE, ESE, SSE, SSW, WSW, WNW, NNW. Any other values are ignored.

[return to overview](map-control.md#basic-usage)
