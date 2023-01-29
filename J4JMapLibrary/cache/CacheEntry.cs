using System.Runtime.CompilerServices;
using J4JSoftware.DeusEx;

namespace J4JMapLibrary;

public class CacheEntry
{
    internal CacheEntry(
        ITiledProjection projection,
        int xTile,
        int yTile,
        byte[] imageData
    )
    {
        Tile = new MapTile( projection, xTile, yTile, imageData );
        CreatedUtc = DateTime.UtcNow;
    }

    internal CacheEntry(
        ITiledProjection projection,
        int xTile,
        int yTile
    )
    {
        Tile = new MapTile( projection, xTile, yTile );
        CreatedUtc = DateTime.UtcNow;
    }

    public MapTile Tile { get; }
    public DateTime CreatedUtc { get; }
    public DateTime LastAccessedUtc { get; set; }
}
