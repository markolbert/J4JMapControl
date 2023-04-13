# J4JMapWinLibrary: J4JMapControl

## Basic usage

- [Basic usage](#basic-usage)
- Properties
  - [Projection properties](#projection-properties)
  - [Map region properties](#map-region-properties)
  - [Overlay control properties](#overlay-control-properties)
  - [Caching properties](#caching-properties)
  - [Miscellaneous proeprties](#miscellaneous-properties)

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

Second, in the code behind you must set the control's `ProjectionFactory` property:

```csharp
mapControl.ProjectionFactory = J4JDeusEx.ServiceProvider.GetService<ProjectionFactory>();

if( mapControl.ProjectionFactory == null )
    _logger?.LogCritical( "ProjectionFactory is not defined" );
```

This example uses my `J4JDeuxEx` generalized view model locator, but you can create a `MapFactory` instance in other ways. Consult the [`J4JMapLibrary` documentation](https://github.com/markolbert/J4JMapControl/tree/main/J4JMapLibrary) for details.

You may also want to set the control's `LoggerFactory` property if you want logging to occur:

```csharp
var loggerFactory = ( (App) Application.Current ).LoggerFactory;

mapControl.LoggerFactory = loggerFactory;
```

Again, this example uses my `J4JDeuxEx` generalized view model locator, but you can create an `ILoggerFactory` instance in other ways.

[return to top](#basic-usage)

## Projection properties

There are two properties which define the mapping service the control uses:

|Property|Type|Comments|
|--------|----|--------|
|`MapProjectionTypes`|`List<string>`|fully qualified type names of custom map projection types (expands the assemblies `ProjectionFactory` searches for supported projection types)|
|`MapProjections`|`List<string>`|the list of available map projections|
|`MapProjection`|`string`|the name of the selected mapping projection|
|`MapStyles`|`List<string>`|the list of map styles available for the selected mapping projection (may be empty if the projection doesn't support styles)|
|`MapStyle`|`string`|the specific style of map projection to use|

If the map projection name is not recognized by `MapFactory` the control's projection is undefined and no imagery will display.

[return to top](#basic-usage)

## Map region properties

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

[return to top](#basic-usage)

## Overlay control properties

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

[return to top](#basic-usage)

## Caching properties

Many, but not all, projections support caching the retrieved imagery. There are a variety of properties available to define how caching works:

|Property|Type|Default|Comments|
|--------|----|-------|--------|
|`UseMemoryCache`|`bool`|`true`|controls whether or not in-memory caching is used when possible|
|`MemoryCacheEntries`|`int`|`DefaultMemoryCacheEntries`|defines how many entries the in-memory cache will retain before it begins purging expired ones|
|`MemoryCacheRetention`|`string`|`string.Empty`|defines how long entries are retained in the in-memory cache.|
|`MemoryCacheSize`|`int`|`DefaultMemoryCacheSize`|defines the maximum size, in bytes, the in-memory cache can be before it begins purging expired entries|
|`FileSystemCachePath`|`string`|`string.Empty`|sets the folder where the `FileSystemCache` stores its files. If undefined, empty or an invalid location file system caching is disabled.|
|`FileSystemCacheEntries`|`int`|`DefaultFileSystemCacheEntries`|defines how many files the file system cache will retain before it begins purging expired ones|
|`FileSystemCacheRetention`|`string`|`string.Empty`|defines how long entries are retained in the file system cache.|
|`FileSystemCacheSize`|`int`|`DefaultFileSystemCacheSize`|defines the maximum size, in bytes, the file system cache can store in files be before it begins purging expired entries|

[return to top](#basic-usage)

## Miscellaneous properties

|Property|Type|Comments|
|--------|----|--------|
|`Annotations`|`List<FrameworkElement>`|holds the FrameworkElements which are displayed above the map as annotations. See the [`MapPin` documentation](map-pin.md) for details.|
|`ShowRotationHints`|`bool`|shows (`true`) or hides (`false`) hints showing how a map's heading is being changed while using click-and-drag to rotate it|
|`MinMapScale`|`double`|sets the minimum map scale allowed. Values outside the range of what the projection supports are adjusted automatically.|
|`MaxMapScale`|`double`|sets the maximum map scale allowed. Values outside the range of what the projection supports are adjusted automatically.|
|`UpdateEventInterval`|`int`|sets the throttling window, in milliseconds, controlling how UI adjustments to map properties are processed, to reduce unnecessary processing and image retrieval. If the supplied value is less than 0 it is set to `DefaultUpdateEventInterval`|

[return to top](#basic-usage)
