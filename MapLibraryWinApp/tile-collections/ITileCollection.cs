using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Foundation;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public interface ITileCollection<TCoord> : ITileCollection
    where TCoord : Coordinates
{
    ReadOnlyCollection<TCoord> Tiles { get; }
    bool TryGetTile( int row, int column, out TCoord? result );
}
