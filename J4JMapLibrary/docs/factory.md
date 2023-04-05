# J4JMapLibrary: Using the Projection Factory

## Overview

- [Ensuring Your Credentials Can Be Found](#ensuring-your-credentials-can-be-found)
- [Creating the Factory](#creating-the-factory)
- [Creating a Projection](#creating-a-projection)
- Custom Projections and Credentials
  - [Scanning for Custom Projections and Credentials](#scanning-for-custom-projections-and-credentials)
  - [Ensuring Custom Projections Can Be Found](#ensuring-custom-projections-can-be-found)
  
While it's fairly straightforward to create a projection, in order to make it possible to do so by choosing one of the caches at run time inspired me to write `ProjectionFactory`.

In order for the factory to work, however, two things are necessary:

- all authentication information must be available through the `IConfiguration` system; and,
- you must have the factory scan whatever assemblies contain `Projection` classes. By default, the factory will scan the library itself, so the four default projections -- Bing Maps, Google Maps, Open Street Maps and Open Topo Maps -- will be included.

This is covered in more detail below.

## Ensuring Your Credentials Can Be Found

To make a projection's credential information available through the `IConfiguration` system, it must be organized in a configuration section called **Credentails**, further identified by the credentials's name. That's so the following code can work:

```csharp
var retVal = Activator.CreateInstance( credType.CredentialsType )!;

var section = _config.GetSection( $"Credentials:{credType.Name}" );
section.Bind( retVal );
```

The built-in credentials have the following names:

|Credential Class|Name|
|----------|----|
|`BingCredentials`|BingMaps|
|`GoogleCredentials`|GoogleMaps|
|`OpenStreetCredentials`|OpenStreetMaps|
|`OpenTopoCredentials`|OpenTopoMaps|

For more details on custom credentials and projections, consult the [documentation on how to create custom projections and credentials](custom-projections.md).

[return to table of contents](#overview)

[return to usage table of contents](usage.md#overview)

## Creating the Factory

The factory constructor takes the following parameters:

|Parameter|Parameter Description|
|---------|---------------------|
|`IConfiguration` config|provides access to the Microsoft configuration system|
|`ILoggerFactory?` loggerFactory = null|optional; provides access to the Microsoft logging factory|
|`bool` includeDefaults = true|default (true) is to scan the library itself|

*You must call `InitializeFactory()` before attempting to use it*. It returns `true` if at least one `Projection` class was found, `false` otherwise. Initialization is somewhat fragile because a lot of reflection is used in order to locate the projection classes and their corresponding credential classes.

[return to table of contents](#overview)

[return to usage table of contents](usage.md#overview)

## Creating a Projection

Once you've initialized the factory you can call one of its creation methods to obtain a projection, all of which return an instance of `ProjectionFactoryResult`. Each comes in both synchronous and asynchronous forms.

### CreateProjection(), CreateProjectionAsync()

|Parameter|Parameter Description|
|---------|---------------------|
|`string` projName|the name of the projection (see below)|
|`ITileCache?` cache = null|the cache to use (optional); ignored for projections which don't support caching|
|`string?` credentialsName = null|the name of the credentials class (optional)|
|`bool` authenticate = true|controls whether or not to authenticate the projection after it's created (default: true)|

### Generic CreateProjection&lt;TProj&gt;(), CreateProjectionAsync&lt;TProj&gt;()

|Parameter|Parameter Description|
|---------|---------------------|
|`TProj`|the projection type (e.g., `BingMapsProjection`)|
|`ITileCache?` cache = null|the cache to use (optional); ignored for projections which don't support caching|
|`string?` credentialsName = null|the name of the credentials class (optional)|
|`bool` authenticate = true|controls whether or not to authenticate the projection after it's created (default: true)|

### Type-based CreateProjection(), CreateProjectionAsync()

|Parameter|Parameter Description|
|---------|---------------------|
|`Type` projType|the projection type (e.g., `typeof(BingMapsProjection)`)|
|`ITileCache?` cache = null|the cache to use (optional); ignored for projections which don't support caching|
|`string?` credentialsName = null|the name of the credentials class (optional)|
|`bool` authenticate = true|controls whether or not to authenticate the projection after it's created (default: true)|

The names of the built-in projections are as follows:

|Projection Class|Name|
|----------|----|
|`BingMapsProjection`|BingMaps|
|`GoogleMapsProjection`|GoogleMaps|
|`OpenStreetMapsProjection`|OpenStreetMaps|
|`OpenTopoMapsProjection`|OpenTopoMaps|

[return to table of contents](#overview)
[return to usage table of contents](usage.md#overview)

## Scanning for Custom Projections and Credentials

To have the factory include custom projection and credential classes you may have written you need to call one of the `ScanAssembly()` methods before calling `InitializeFactory()`. The `ScanAssemblies()` methods return the `ProjectionFactory` itself, so they can be daisy-chained.

[return to table of contents](#overview)

[return to usage table of contents](usage.md#overview)

## Ensuring Custom Projections Can Be Found

If you create a custom projection class you must deocrate it with a `ProjectionAttribute` in order for the factory to find it when it scans the assembly where it's defined. Here's an example of how the built-in `BingMapsProjection` class is named:

```csharp
[ Projection( "BingMaps" ) ]
public sealed class BingMapsProjection : TiledProjection<BingCredentials>
```

Projection names must be unique within the application's environment, so be sure not to duplicate any of the built-in names:

|Projection Class|Name|
|----------|----|
|`BingMapsProjection`|BingMaps|
|`GoogleMapsProjection`|GoogleMaps|
|`OpenStreetMapsProjection`|OpenStreetMaps|
|`OpenTopoMapsProjection`|OpenTopoMaps|

For more details on custom credentials and projections, consult the [documentation on how to create custom projections and credentials](custom-projections.md).

[return to table of contents](#overview)

[return to usage table of contents](usage.md#overview)
