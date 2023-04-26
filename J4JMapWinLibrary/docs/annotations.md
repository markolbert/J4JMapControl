# J4JMapControl: Annotations

## Introduction

The map control can display two kinds of annotations:

- a collection of `FrameworkElements`, each of which must be tagged with a location property; and,
- a collection defined by a data source and a `DataTemplate`

## FrameworkElement Annotations

The first type is configured like this:

```xml
<map:J4JMapControl x:Name="mapControl" 
                    Grid.Column="0" Grid.Row="0" Grid.ColumnSpan="3"
                    Margin="5"
                    MapProjection="BingMaps"
                    MapScale="{Binding ElementName=numberBoxScale, Path=Value, Mode=TwoWay}"
                    Heading="{Binding ElementName=numberBoxHeading, Path=Value, Mode=TwoWay}"
                    Center="37.5072N,122.2605W"
                    PoILatLong="Location"
                    PoIDataSource="{x:Bind _ptsOfInterest}">

    <map:J4JMapControl.Annotations>
        
        <map:MapPin ArcRadius="15"
                    TailLength="30"                      
                    map:Location.LatLong="37.5072N,122.2605W" 
                    HorizontalAlignment="Center"
                    VerticalAlignment="Bottom" />

          <!--this shouldn't show up--> 
        <Rectangle Width="50" Height="50" Fill="Green"/>

    </map:J4JMapControl.Annotations>
```

Each `FrameworkElement` must be tagged with the attached property `Location.LatLong` which defines where on the map the visual element should be displayed.

`Location` offers other properties as well as `LatLong`:

|Property|Type|Comments|
|--------|----|--------|
|`Latitude`|`string`|a [parseable latitude value](parseable-location-formats.md)|
|`Longitude`|`string`|a [parseable longitude value](parseable-location-formats.md)|
|`LatLong`|`string`|a [parseable latitude/longitude value](parseable-location-formats.md)|
|`Offset`|`string`|a parseable offset string defining how many pixels the visual element should be offset from the latitude/longitude position (e.g., "5, -5")|

If `Latitude` and `Longitude` are both valid values they will override any value assigned to `LatLong`.

The visual element's position also takes into account `HorizontalAlignment` and `VerticalAlignment` values, if those are specified. `HorizontalAlignment` defaults to `Center` while `VerticalAlignment` defaults to `Middle`.

[return to top](#introduction)

[return to overview](map-control.md#basic-usage)

## Templated Annotations

Templated annotations are defined by setting several properties on the map control:

|Property|Type|Comments|
|--------|----|--------|
|`PoIDataSource`|object|an enumerable holding items defining the templated items to be constructed. Each record must at least contain location data.|
|`PoILatitude`|`string`|the name of the property in the data source which holds [parseable latitude values](parseable-location-formats.md)|
|`PoILongitude`|`string`|the name of the property in the data source which holds [parseable longitude values](parseable-location-formats.md)|
|`PoILatLong`|`string`|the name of the property in the data source which holds [parseable latitude/longitude values](parseable-location-formats.md)|

If `PoIDataSource` is not an `IEnumerable` it will be ignored and no templated annotations will be created. The UI will update to reflect changes to the collection `PoIDataSource` points to.

If `PoILatitude` and `PoILongitude` are both valid property names that contain valid values they will override the use of data contained in a `PoILatLong` property, even if it is separately defined.

[return to top](#introduction)

[return to overview](map-control.md#basic-usage)
