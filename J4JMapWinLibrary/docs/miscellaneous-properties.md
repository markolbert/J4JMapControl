# J4JMapControl: Miscellaneous properties

|Property|Type|Comments|
|--------|----|--------|
|`MaxControlHeight`|`double`|sets the maximum height of the compass rose/scale slider region. Defaults to unlimited.|
|`ShowRotationHints`|`bool`|shows (`true`) or hides (`false`) hints showing how a map's heading is being changed while using click-and-drag to rotate it|
|`MinMapScale`|`double`|sets the minimum map scale allowed. Values outside the range of what the projection supports are adjusted automatically.|
|`MaxMapScale`|`double`|sets the maximum map scale allowed. Values outside the range of what the projection supports are adjusted automatically.|
|`UpdateEventInterval`|`int`|sets the throttling window, in milliseconds, controlling how UI adjustments to map properties are processed, to reduce unnecessary processing and image retrieval. If the supplied value is less than 0 it is set to `DefaultUpdateEventInterval`|

[return to overview](map-control.md#basic-usage)
