namespace J4JMapLibrary;

public class CacheEntry
{
    internal CacheEntry(
        ITiledProjection projection,
        int xTile,
        int yTile,
        int scale,
        byte[] imageData
    )
    {
        Tile = new TiledFragment( projection, xTile, yTile, scale, imageData );
        CreatedUtc = DateTime.UtcNow;
    }

    internal CacheEntry(
        ITiledProjection projection,
        int xTile,
        int yTile,
        int scale,
        CancellationToken ctx
    )
    {
        Tile = TiledFragment.CreateAsync( projection, xTile, yTile, scale, true, ctx ).Result;
        CreatedUtc = DateTime.UtcNow;
    }

    public TiledFragment Tile { get; }
    public DateTime CreatedUtc { get; }
    public DateTime LastAccessedUtc { get; set; }
    public bool ImageIsLoaded => Tile.ImageBytes > 0L;
}
