# J4JMapControl: Overlay control properties

`J4JMapControl` can display two controls above the map imagery, a compass rose showing the map's heading, and a slider control which can be used to adjust the map's scale.

There are a variety of properties which can be used to fine-tune these controls:

|Property|Type|Comments|
|--------|----|--------|
|`ControlVisibility`|`bool`|`true` displays the overlay controls, `false` hides them|
|`HorizontalControlAlignment`|`HorizontalAlignment`|controls where the compass rose and map scale control are displayed horizontally within the map control. Default is `Right`.|
|`VerticalControlAlignment`|`VerticalAlignment`|controls where the compass rose and map scale control are displayed vertically within the map control. Default is `Top`.|
|`CompassRoseImage`|`BitmapImage`|the image used for the compass rose. It should be square so that scaling does not distort it.|
|`CompassRoseHeightWidth`|`double`|the height/width of the compass rose control.|
|`ControlBackground`|`Color`|the color of the `Brush` used to fill the overlay controls' background. Default is ARGB(255, 128, 128, 128).|
|`ControlBackgroundOpacity`|`double`|the opacity of the overlay controls' background. Valid values are 0 to 1.0.|
|`ControlVerticalMargin`|`double`|controls the vertical spacing between the compass rose and the map scale slider controls|
|`LargeMapScaleChange`|`double`|defines what constitutes a "large" map scale change for the map scale slider control|

[return to overview](map-control.md#basic-usage)
