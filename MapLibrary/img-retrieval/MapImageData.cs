using Windows.Storage.Streams;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.MapLibrary;

public record MapImageData( MapTile MapTile, InMemoryRandomAccessStream Stream );
