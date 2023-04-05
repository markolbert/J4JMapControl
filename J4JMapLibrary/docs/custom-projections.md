# J4JMapLibrary: Creating Custom Projections

A projection's name is defined by the `ProjectionAttribute` that decorates it:

```csharp
[ Projection( "BingMaps" ) ]
public sealed class BingMapsProjection : TiledProjection<BingCredentials>
{
    //...
}
```
