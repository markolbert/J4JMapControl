# J4JMapLibrary: Using the Projection Factory

## Overview

- [Creating the Factory](#creating-the-factory)
- [Creating a Projection](#creating-a-projection)
- [Authentication](authentication.md)
- Custom Projections and Credentials
  - [Scanning for Custom Projections and Credentials](#scanning-for-custom-projections-and-credentials)
  - [Ensuring Custom Projections Can Be Found](#ensuring-custom-projections-and-credentials-can-be-found)
  
While it's fairly straightforward to create a projection, in order to make it possible to do so by choosing one of the caches at run time inspired me to write `ProjectionFactory`.

The basic process for using it is:

- create and instance of `ProjectionFactory`
- tell it whatever custom projection/credentials assemblies you want it to search (by default, it automatically searches the `J4JMapLibrary` assembly, although you can keep it from doing so by specifying a flag when you create the instance)
- call the `InitializeFactory()` method
- use one of the `Create...` methods to create a projection

This is covered in more detail below. Note that before a projection can be used it must be authenticated. Consult the [authentication docs](authentication.md) to learn more about how to do this.

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

Once you've initialized the factory you can call one of its creation methods to obtain a projection:

```csharp
public IProjection? CreateProjection( string projName )

public IProjection? CreateProjection<TProj>()
        where TProj : IProjection

public IProjection? CreateProjection( Type projType )
```

The built-in projections have the following names:

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

## Ensuring Custom Projections and Credentials Can Be Found

If you create a custom projection class you must deocrate it with a `ProjectionAttribute` in order for the factory to find it when it scans the assembly where it's defined. Here's an example of how the built-in `BingMapsProjection` class is named:

```csharp
[ Projection( "BingMaps" ) ]
public sealed class BingMapsProjection : TiledProjection<BingCredentials>
```

Projection names must be unique within the application's environment, so be sure not to duplicate any of the built-in names.

Custom credentials for a custom projection/map service must implement the `ICredentials` interface:

```csharp
public interface ICredentials : INotifyPropertyChanged
{
    Type ProjectionType { get; }
    string ProjectionName { get; }
    IEnumerable<Credentials.CredentialProperty> CredentialProperties { get; }

    ICredentials Encrypt( IDataProtector protector);
    ICredentials Decrypt( IDataProtector protector );
}
```

To simplify doing this you can derive your custom credentials class from the abstract class `Credentials`.

`Credentials` assumes your custom projection class is decorated with a `ProjectionAttribute`, which it uses to look up the unique name of the projection.

Custom credentials classes can implement whatever properties they need to for that projection's authentication. For more details on custom credentials and projections, consult the [documentation on how to create custom projections and credentials](custom-projections.md).

[return to table of contents](#overview)

[return to usage table of contents](usage.md#overview)
