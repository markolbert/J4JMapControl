using Windows.Storage.Streams;

namespace J4JSoftware.MapLibrary;

public record MapImageData( MapTile MapTile, InMemoryRandomAccessStream Stream );
