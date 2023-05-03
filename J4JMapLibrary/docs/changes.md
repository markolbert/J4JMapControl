# J4JMapLibrary: Change Log

|Version|Comments|
|:-----:|--------|
|0.9.2|fixed authentication problems|
|0.9.0|**breaking changes**, [see details below](#v090)|
|0.8.4|bumped to align with J4JMapWinLibrary|
|0.8.3|fixed problems with various mapping services|
|0.8.2|fixed caching problems|
|0.8.1|changed how caching works, updated nuget dependencies|
|0.8|Initial release|

## v0.9.0

Adding the ability to specify credentials at run-time, as opposed to just through a configuration system, exposed some problems with the `ProjectionFactory` design. This was particularly true with authenticating projections using credentials provided at run-time from a WinUI3 app. Since that environment is the one I was targeting changes had to be made :).

`ProjectionFactory` no longer offers the opportunity to authenticate the projections it creates. That must now be done by the code using the projection. To aid in that process there is now a `CredentialsFactory` which returns the necessary objects (i.e., since each projection has its own unique set of credentialing parameters).

The various methods for creating projections from `ProjectionFactory` have been simplified.

`CredentialsFactory` can optionally initialize credentials objects from the `IConfiguration` system, provided you've added the necessary information to it. The WinUI3 test app contained in the [repository](https://github.com/markolbert/J4JMapControl), **WinAppTest**, shows one way of doing this.
