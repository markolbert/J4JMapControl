using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public class PixelTileLatLongTiles : TilesBase<PixelTileLatLong>
{
    public PixelTileLatLongTiles(IJ4JLogger? logger)
        : base(logger)
    {
    }

    protected override List<PixelTileLatLong> GetRelevantTiles()
    {
        var retVal = new List<PixelTileLatLong>();

        for (var yTile = UpperLeft!.Tile.Y; yTile < LowerRight!.Tile.Y; yTile++)
        {
            for (var xTile = UpperLeft!.Tile.X; xTile < LowerRight!.Tile.X; xTile++)
            {
                var curTile = Tiles
                       .FirstOrDefault(t => t.Tile.X == xTile && t.Tile.Y == yTile)
                 ?? UpperLeft.Zoom.ToMultiTileCoordinates(new IntPoint(xTile, yTile));

                retVal.Add(curTile);
            }
        }

        return retVal;
    }

    public override bool TryGetTile(int row, int column, out PixelTileLatLong? result)
    {
        result = Tiles.FirstOrDefault(t => t.Tile.X == column
                                               && t.Tile.Y == row);

        return result != null;
    }
}