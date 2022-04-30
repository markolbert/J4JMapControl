using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Foundation;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public interface ITileCollection
{
    int NumTiles { get; }
    int NumRows { get; }
    int NumColumns { get; }

    MapPoint? UpperLeft { get; }
    MapPoint? LowerRight { get; }

    void Update( MapRect viewPort );
    bool TryGetTile( int row, int column, out TileCoordinates? result );
}

public interface ITileCollection<TCoord> : ITileCollection
    where TCoord : TileCoordinates
{
    ReadOnlyCollection<TCoord> Tiles { get; }

    bool TryGetTile( int row, int column, out TCoord? result );
}
