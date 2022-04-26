using System.Collections.Generic;
using Windows.Foundation;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public interface ITileCollection
{
    IEnumerable<TileCoordinates> GetRelevantTiles( Rect viewPort, IZoom zoom );
}
