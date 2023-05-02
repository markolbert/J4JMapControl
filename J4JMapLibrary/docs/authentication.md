# J4JMapLibrary: Authentication

- [Overview](#overview)
- [The ICredentials Interface](#the-icredentials-interface)
- [The Credentials Factory](#the-credentials-factory)
  - [Retrieving Credentials](#retrieving-credentials)
- [IConfiguration Details](#iconfiguration-details)
  - [Background](#background)
  - [Loading the IConfiguration System](#loading-the-iconfiguration-system)
  
## Overview

After creating a projection instance you call one of its authentication methods to enable using it:

```csharp
public bool SetCredentials( TAuth credentials )

public async Task<bool> SetCredentialsAsync( 
    TAuth credentials, 
    CancellationToken ctx = default )
```

A projection created by the factory will not be able to retrieve map images until it is *authenticated*. Each mapping service uses a different set of information to authenticate use. Some -- **Open Street Maps** and **Open Topo Maps** -- merely require a unique identifying string. Others, like **Google Maps**, rely on a key you get by setting up an account with the service. **Bing Maps** goes even further, requiring approval from a website -- a *different* website than the one used to retrieve images -- using a key you get by setting up an account with the service.

Each service uses a different credentials class, each of which is dervied from `Credentials` and implements the `ICredentials` interface:

|Service|TAuth Class|Details|
|-------|-----------|-------|
|Bing Maps|`BingCredentials`|`ApiKey` property holds API key|
|Google Maps|`GoogleCredentials`|`ApiKey` property holds API key|
|||`SignatureSecret` property holds your signature secret|
|Open Street Maps|`OpenStreetCredentials`|`UserAgent` property holds user agent string|
|Open Topo Maps|`OpenTopoCredentials`|`UserAgent` property holds user agent string|

The authentication methods return true if they succeed, false if they don't.

The only currently-supported service where authentication might fail is Bing Maps, because Bing Maps requires interaction with the web to complete authentication and acquire various pieces of information required to access the service. That can fail for many reasons, including inability to access the web.

Invalid credentials will cause failures when map images are retrieved, but that's a separate issue.

[return to table of contents](#overview)

[return to usage table of contents](usage.md#overview)

## The ICredentials Interface

Every credentials object must implement `ICredentials` (to simplify that, it's best to derive them from `Credentials`):

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

Various parts of the library need to access the authentication properties in a credentials object without knowing precisely which credentials object they're dealing with. `CredentialProperties` provides that information. The properties and their values it returns are defined by which credential properties are decorated with `CredentialPropertyAttribute`. Here's what `BingCredentials` looks like (some details omitted for clarity):

```csharp
public class BingCredentials : Credentials
{
    public BingCredentials()
        : base( typeof( BingMapsProjection ) )
    {
    }

    [ CredentialProperty ]
    public string ApiKey
    {
        get => _apiKey;
        set => SetField( ref _apiKey, value );
    }
}
```

It's not good practice to store credential information in clear text, so the library supports using the `IDataProtector` system to encrypt and decrypt credentials.

This is done through the `Encrypt()` and `Decrypt()` methods defined in the `ICredentials` interface. Each uses the `IDataProtector` system to encrypt or decrypt a credentials object. Note that they **don't** modify the current instance but instead return a **new** instance of the credentials class.

[return to table of contents](#overview)

[return to usage table of contents](usage.md#overview)

## The Credentials Factory

The various credential objects are simple enough you could simply create them as needed. However, to make things easier, the library can create credential objects for you through the `CredentialsFactory`. When you use the factory you will also gain the ability to initialize credentials from the `IConfiguration` system.

Using the `CredentialsFactory` involves the following steps:

- create an instance of `CredentialsFactory`
- tell it where to search for credential classes (by calling its `ScanAssemblies()` methods)
- initialize it (by calling its `InitializeFactory()` method)
- [retrieve credentials](#retrieving-credentials) by name or type (see below)

The constructor has one required and two optional properties:

```csharp
public CredentialsFactory(
    IConfiguration config,
    ILoggerFactory? loggerFactory = null,
    bool includeDefaults = true
)
```

If you don't provide an `ILoggerFactory` the factory won't log events. `includeDefaults` controls whether or not the factory searches for credential classes in its own assembly. It defaults to `true` because the four supported services all have credential classes defined in the same assembly.

The two forms of `ScanAssemblies()` control what assemblies get searched for credential classes. The one that takes an array of `Types` searches the assemblies in which the types are defined:

```csharp
public CredentialsFactory ScanAssemblies( params Type[] types )

public CredentialsFactory ScanAssemblies( params Assembly[] assemblies )
```

### Retrieving Credentials

You can retrieve credential objects by either projection name or *projection* type (not credentials type).

```csharp
public ICredentials? this[ string projName, bool initializeFromConfig = true ]

public ICredentials? this[Type projType, bool initializeFromConfig = true]
```

In either case, you can have the factory attempt to initialize the credentials object from the `IConfiguration` system. Initialization is not guaranteed to succeed because it depends on you loading `IConfiguration` with the correct information. See the section on [using `IConfiguration`](#iconfiguration-details) below for details.

[return to table of contents](#overview)

[return to usage table of contents](usage.md#overview)

## `IConfiguration` Details

### Background

`IConfiguration` works by storing configuration values as plain-text strings, keyed by a *property path* which defines which property should be initialized with the stored value. For example, a class whose value you want available in the `IConfiguration` system that looks like this:

```csharp
public class SomeConfigClass
{
    public string? SomeText { get; set; }
    public int SomeInteger { get; set; }
}
```

might have its values stored in the `IConfiguration` system like this:

|`IConfiguration` Key|Value|
|--------------------|-----|
|"SomeConfigClass:SomeText"|"some value"|
|"SomeConfigClass:SomeInteger"|37|

### Loading the IConfiguration System

A problem arises because the `IConfiguration` keys are tied to an explicit path to a property. In effect, they are "magic strings"...which the library can't be expected to know.

One workaround is to require all map credential information to be contained in a property with a specific, known-in-advance name. For various reasons I decided not to go this route.

Instead, the library provides methods for converting the authentication properties of any `ICredentials` object into a known set of key/value pairs. This is implemented as an extension method on the `ICredentials` interface:

```csharp
public static IEnumerable<KeyValuePair<string, string?>> ToKeyValuePairs( this ICredentials credentials )
```

`ToKeyValuePairs()` will only generate key/value pairs for *public* properties decorated with `CredentialPropertyAttribute`.

You typically call this someplace in the startup code for you application,based on some global configuration object you've retrieved or created (this example snippet comes from the `WinAppTest` project contained in the repository):

```csharp
// path is the location of a configuration file stored on disk
var encrypted = JsonSerializer.Deserialize<AppConfiguration>(File.ReadAllText( path ));

if( encrypted == null )
{
    _hostConfiguration!.Logger.Error( "Could not parse user config file '{path}'", path );
    return retVal;
}

retVal = encrypted.Decrypt( _hostConfiguration!.DataProtector );

retVal.AddToConfiguration( builder );
```

The `AddToConfiguration()` method in the example is defined like this (some details omitted for clarity):

```csharp
public void AddToConfiguration( IConfigurationBuilder builder )
{
    var keyValuePairs = new List<KeyValuePair<string, string?>>();

    // Credentials is a simple class which contains a property for
    // each of the supported credential classes
    if( Credentials is { GoogleCredentials: not null } )
        keyValuePairs.AddRange( Credentials.GoogleCredentials.ToKeyValuePairs() );

    if( Credentials is { BingCredentials: not null } )
        keyValuePairs.AddRange( Credentials.BingCredentials.ToKeyValuePairs() );

    if( Credentials is { OpenStreetCredentials: not null } )
        keyValuePairs.AddRange( Credentials.OpenStreetCredentials.ToKeyValuePairs() );

    if( Credentials is { OpenTopoCredentials: not null } )
        keyValuePairs.AddRange( Credentials.OpenTopoCredentials.ToKeyValuePairs() );

    builder.AddInMemoryCollection( keyValuePairs );
}
```

[return to table of contents](#overview)

[return to usage table of contents](usage.md#overview)
