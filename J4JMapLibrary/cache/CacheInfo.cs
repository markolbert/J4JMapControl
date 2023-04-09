using J4JSoftware.J4JMapLibrary;

public record CacheInfo( int Level, ITileCache Cache )
{
    public string Name => Cache.Name;
}
