# J4JMapWinLibrary: MapPin

## Basic Usage

- [Basic usage](#basic-usage)
- [Formatting properties](#formatting-properties)

The `MapPin` control displays an image similar to this:

![map pin](assets/map-pin.png)

To locate it on the map control you use the following *annotation properties*:

|Property|Type|Default|Comments|
|--------|----|-------|--------|
|`Location.Center`|`string`|`null`|must be a parseable latitude/longitude string. See the discussion on [map region properties](map-control#map-region-properties) for details.|
|`Location.Offset`|`string`|"0,0"|the horizontal and vertical offsets, in pixels, to apply to `MapPin` when it is positioned on the map control. See below for details.|

The `Location.Offset` property must be in the following format:

- horizontal offset numeric value (`float`)
- a separating comma or space
- vertical offset numeric value (`float`)

If the value doesn't match the format `Location.Offset` defaults to **0, 0**.

[return to top](#basic-usage)

## Formatting properties

There are several properties which control what the `MapPin` looks like:

|Property|Type|Default|Comments|
|--------|----|-------|--------|
|`ArcRadius`|`double`|15|the radius of the circle forming the top of the image. Values less than 0 default to 15.|
|`TailLength`|`double`|30|the height of the triangle forming the bottom of the image. Values less than 0 default to 30|
|`Fill`|`Brush`|a solid red brush||
|`Stroke`|`Brush`|a transparent brush||
|`StrokeThickness`|`double`|0|values less than 0 default to 0|

[return to top](#basic-usage)
