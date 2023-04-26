# J4JMapControl: Projection properties

There are two properties which define the mapping service the control uses:

|Property|Type|Comments|
|--------|----|--------|
|`MapProjectionTypes`|`List<string>`|fully qualified type names of custom map projection types (expands the assemblies `ProjectionFactory` searches for supported projection types)|
|`MapProjections`|`List<string>`|the list of available map projections|
|`MapProjection`|`string`|the name of the selected mapping projection|
|`MapStyles`|`List<string>`|the list of map styles available for the selected mapping projection (may be empty if the projection doesn't support styles)|
|`MapStyle`|`string`|the specific style of map projection to use|

If the map projection name is not recognized by `MapFactory` the control's projection is undefined and no imagery will display.

[return to overview](map-control.md#basic-usage)
