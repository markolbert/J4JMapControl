using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public class ScreenGlobalTiles : TilesBase<ScreenGlobalCoordinates>
{
    public ScreenGlobalTiles(IJ4JLogger? logger)
        : base(logger)
    {
    }

    protected override List<ScreenGlobalCoordinates> GetRelevantTiles()
    {
        var retVal = new List<ScreenGlobalCoordinates>();

        var ulScreenPt = UpperLeft!.Screen.ToDoublePoint();
        var lrScreenPt = LowerRight!.Screen.ToDoublePoint();
        var zoom = UpperLeft.Zoom;

        retVal.Add( new ScreenGlobalCoordinates( ulScreenPt,
                                               lrScreenPt,
                                               zoom.ScreenToLatLong(ulScreenPt),
                                               zoom.ScreenToLatLong(lrScreenPt),
                                               UpperLeft.Zoom ) );

        return retVal;
    }

    public override bool TryGetTile( int row, int column, out ScreenGlobalCoordinates? result )
    {
        // only ever one tile...
        result = Tiles.FirstOrDefault();

        return result != null;
    }
}
