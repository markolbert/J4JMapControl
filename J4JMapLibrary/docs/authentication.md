# J4JMapLibrary: Authentication

After creating a projection instance you call one of its authentication methods to enable using it:

```csharp
public bool SetCredentials( TAuth credentials )

public async Task<bool> SetCredentialsAsync( 
    TAuth credentials, 
    CancellationToken ctx = default )
```

Each service uses a different `TAuth` class:

|Service|TAuth Class|Details|
|-------|-----------|-------|
|Bing Maps|`BingCredentials`|`ApiKey` property holds API key|
|Google Maps|`GoogleCredentials`|`ApiKey` property holds API key<br>`SignatureSecret` property holds your signature secret|
|Open Street Maps|`OpenStreetCredentials`|`UserAgent` property holds user agent string|
|Open Topo Maps|`OpenTopoCredentials`|`UserAgent` property holds user agent string|

The authentication methods return true if they succeed, false if they don't.

The only service where authentication might fail is Bing Maps, because Bing Maps requires interaction with the web to complete authentication and acquire various pieces of information required to access the service. That can fail for many reasons, including inability to access the web.

[return to table of contents](usage.md#overview)
