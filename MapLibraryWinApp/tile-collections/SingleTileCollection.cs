using System;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public class SingleTileCollection : TileCollection<MultiTileCoordinates>
{
    public SingleTileCollection(IJ4JLogger? logger)
        : base(logger)
    {
    }

    protected override void UpdateInternal()
    {
        throw new NotImplementedException();
    }

    public override bool TryGetTile( int row, int column, out MultiTileCoordinates? result )
    {
        result = null;
        return false;
    }

    //public IEnumerable<TileCoordinates> GetRelevantTiles(Rect viewPort, IZoom zoom)
    //{
    //    var upperLeftScreenPt = new IntPoint(RoundToInt(viewPort.Left), RoundToInt(viewPort.Top));
    //    var lowerRightScreenPt = new IntPoint(RoundToInt(viewPort.Right), RoundToInt(viewPort.Bottom));

    //    var upperLeftLatLong = zoom.ToLatLong(upperLeftScreenPt);
    //    var lowerRightLatLong = zoom.ToLatLong(lowerRightScreenPt);

    //    yield return new SingleTileCoordinates(upperLeftScreenPt,
    //                                           lowerRightScreenPt,
    //                                           upperLeftLatLong,
    //                                           lowerRightLatLong,
    //                                           zoom);
    //}

}
