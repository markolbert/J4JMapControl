using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.MapLibrary;

public interface ITileCollection
{
    int NumTiles { get; }
    int NumRows { get; }
    int NumColumns { get; }

    double ScreenWidth { get; }
    double ScreenHeight { get; }

    MapPoint? UpperLeft { get; }
    MapPoint? LowerRight { get; }

    void Update(MapRect viewPort);

    Coordinates? this[int row, int column] { get; }
    bool TryGetTile(int row, int column, out Coordinates? result);
}