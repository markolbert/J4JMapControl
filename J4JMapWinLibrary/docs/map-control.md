# J4JMapWinLibrary: J4JMapControl

## Basic usage

- [Projection properties](projection-properties.md)
- [Map region properties](map-region-properties.md)
- [Overlay control properties](overlay-control-properties.md)
- [Caching properties](caching-properties.md)
- [Annotations](annotations.md)
- [Routes](routes.md)
- [Miscellaneous proeprties](miscellaneous-properties.md)

*The test project `WinAppTest`, also contained in this repository, is a working example of using `J4JMapControl`.*

`J4JMapControl` supports:

- clicking and dragging to move the center point horizontally/vertically.
- rotating the map around a point by holding down the *control* key while clicking and dragging.
- using the mouse wheel to change the scale of the map

Using the control involves two steps. First, you must add it to an XAML file and set at least some of its properties:

```xml
<map:J4JMapControl x:Name="mapControl" 
                    Grid.Column="0" Grid.Row="0" Grid.ColumnSpan="3"
                    MapProjection="BingMaps"
                    MapScale="13"
                    Heading="45"
                    Center="37.5072N,122.2605W">
```

Second, in the code behind you must set the control's `MapProjectionFactory` property:

```csharp
mapControl.MapProjectionFactory = J4JDeusEx.ServiceProvider.GetService<ProjectionFactory>();

if( mapControl.MapProjectionFactory == null )
    _logger?.LogCritical( "MapProjectionFactory is not defined" );
```

By default, `ProjectionFactory` searches its own assembly for map projection objects. You can customize it to search other assemblies by calling one of the following methods on it before you assign it to `J4JMapControl`:

|Method|Argument(s)|Comments|
|------|-----------|--------|
|`ScanAssemblies`|`params Type[] types`|searches every assembly containing one of the specified types|
|`ScanAssemblies`|`params Assembly[] assemblies`|searches the specified assemblies|

This example uses my `J4JDeuxEx` generalized view model locator, but you can create a `MapFactory` instance in other ways. Consult the [`J4JMapLibrary` documentation](https://github.com/markolbert/J4JMapControl/tree/main/J4JMapLibrary) for details.

You may also want to set the control's `LoggerFactory` property if you want logging to occur:

```csharp
var loggerFactory = ( (App) Application.Current ).LoggerFactory;

mapControl.LoggerFactory = loggerFactory;
```

Again, this example uses my `J4JDeuxEx` generalized view model locator, but you can create an `ILoggerFactory` instance in other ways.

[return to top](#basic-usage)
