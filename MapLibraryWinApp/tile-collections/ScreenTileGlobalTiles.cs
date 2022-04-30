using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public class ScreenTileGlobalTiles : TilesBase<ScreenTileGlobalCoordinates>
{
    public ScreenTileGlobalTiles(IJ4JLogger? logger)
        : base(logger)
    {
    }

    protected override List<ScreenTileGlobalCoordinates> GetRelevantTiles()
    {
        var retVal = new List<ScreenTileGlobalCoordinates>();

        for (var yTile = UpperLeft!.Tile.Y; yTile < LowerRight!.Tile.Y; yTile++)
        {
            for (var xTile = UpperLeft!.Tile.X; xTile < LowerRight!.Tile.X; xTile++)
            {
                var curTile = Tiles
                       .FirstOrDefault(t => t.TileCoordinates.X == xTile && t.TileCoordinates.Y == yTile)
                 ?? UpperLeft.Zoom.ToMultiTileCoordinates(new IntPoint(xTile, yTile));

                retVal.Add(curTile);
            }
        }

        return retVal;
    }

    public override bool TryGetTile(int row, int column, out ScreenTileGlobalCoordinates? result)
    {
        result = Tiles.FirstOrDefault(t => t.TileCoordinates.X == column
                                               && t.TileCoordinates.Y == row);

        return result != null;
    }
}