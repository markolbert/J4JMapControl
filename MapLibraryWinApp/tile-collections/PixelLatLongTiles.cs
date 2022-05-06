using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public class PixelLatLongTiles : TilesBase<PixelLatLong>
{
    public PixelLatLongTiles(IJ4JLogger? logger)
        : base(logger)
    {
    }

    protected override List<PixelLatLong> GetRelevantTiles()
    {
        var retVal = new List<PixelLatLong>();

        var ulScreenPt = UpperLeft!.TileRelativePixel.ToDoublePoint();
        var lrScreenPt = LowerRight!.TileRelativePixel.ToDoublePoint();
        var zoom = UpperLeft.Zoom;

        retVal.Add( new PixelLatLong( ulScreenPt,
                                               lrScreenPt,
                                               zoom.RelativePointToLatLong(ulScreenPt),
                                               zoom.RelativePointToLatLong(lrScreenPt),
                                               UpperLeft.Zoom ) );

        return retVal;
    }

    public override bool TryGetTile( int row, int column, out PixelLatLong? result )
    {
        // only ever one tile...
        result = Tiles.FirstOrDefault();

        return result != null;
    }
}
