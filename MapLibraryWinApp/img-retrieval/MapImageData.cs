using Windows.Storage.Streams;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public record MapImageData( Coordinates Coordinates, InMemoryRandomAccessStream Stream );
