namespace J4JMapLibrary;

public interface ICacheEntry
{
    MapTile Tile { get; }
    DateTime CreatedUtc { get; }
    DateTime LastAccessedUtc { get; set; }
}
