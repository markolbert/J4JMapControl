# J4JMapControl: Routes

`J4JMapControl` can display routes -- collections of locations which define paths on a map -- by setting various *route properties*.

## Location Data Structure

The data defining the locations must follow a particular structure. You can name the fields whatever you like (you specify the property names in the XAML), but the required data must be available in the location collection.

Because the latitude/longitude data can be provided in different ways not all of the properties are required to be present for each item in the location collection:

|Property|Type|Comments|
|--------|----|--------|
|latitude|`string`|must be in a [parseable format](parseable-location-formats.md)|
|longitude|`string`|must be in a [parseable format](parseable-location-formats.md)|
|latlong|`string`|combined latitude/longitude data. Must be in a [parseable format](parseable-location-formats.md)|
|visibile|`bool`|(optional) determines whether a marker is displayed for the location|

*A valid location must contain a latitude and a longitude property --or-- a latlong property*. The validity of each location is determined separately, so some can have latitude and longitude defined while others only have latlong defined.

[return to top](#j4jmapcontrol-routes)

[return to overview](map-control.md#basic-usage)

## Route Properties

A route is configured by specifying a number of properties. Here's how one might look in XAML:

```xml
<map:J4JMapControl x:Name="mapControl" 
                    Grid.Column="0" Grid.Row="0" Grid.ColumnSpan="3">

    <map:J4JMapControl.MapRoutes>
        <map:MapRoute RouteName="Route1" 
                        LatitudeField="Latitude" 
                        LongitudeField="Longitude" 
                        StrokeColor="Red"
                        ShowPoints="True"
                        PointVisibilityField="Visible"
                        DataSource="{x:Bind _route1}"/>

        <map:MapRoute RouteName="Route2" 
                        LatitudeField="Latitude" 
                        LongitudeField="Longitude" 
                        StrokeColor="Green"
                        ShowPoints="True"
                        PointVisibilityField="Visible"
                        DataSource="{x:Bind _route2}"/>
    </map:J4JMapControl.MapRoutes>

</map:J4JMapControl>
```

|Property|Type|Comments|
|--------|----|--------|
|`RouteName`|`string`|**required**, even if you only specify one route. Identifies the route.|
|`LatitudeField`|`string`|the name of the field in your location object which contains latitude information|
|`LongitudeField`|`string`|the name of the field in your location object which contains longitude information|
|`LatLongField`|`string`|the name of the field in your location object which contains combined latitude/longitude information|
|`PointVisibilityField`|`string`|the name of the field in your location object which contains a boolean indicating whether a particular location should have a marker displayed|
|`DataSource`|`object`|the locations collection. *If it is not an `IEnumerable` nothing will be displayed*.|
|`PointTemplate`|`DataTemplate`|a template for creating the marker for a location. If no template is specified a filled circle will be displayed if required.|
|`StrokeColor`|`Color`|the color of the route's path. Defaults to black.|
|`StrokeWidth`|`double`|the width of the route's path. Defaults to 5 pixels.|
|`ShowPoints`|`bool`|`True` means markers will be displayed if the location's `PointVisibilityField` is defined and resolves to true. `False` means no markers will be display, regardless of whether the `PointVisibilityField` is defined and regardless of what it resolves to.|
|`StrokeOpacity`|`double`|the opacity of the path and the markers. Defaults to 0.30. Values less than 0 are changed to 0.30. Values greater than 1.0 are changed to 1.0.|

A route's visual display will update when any of the following fields are changed:

- `StrokeColor`
- `StrokeWidth`
- `ShowPoints`
- `StrokeOpacity`

It will also update if the data source implements `INotifyCollectionChanged` and changes are made to the collection, or if a location item implements `INotifyPropertyChanged` for a field and the field's value changes.

[return to top](#j4jmapcontrol-routes)

[return to overview](map-control.md#basic-usage)
