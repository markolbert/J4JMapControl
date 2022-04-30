using System.Collections.Generic;
using Windows.Foundation;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public class MultiTileCollection : TileCollection<MultiTileCoordinates>
{
    public MultiTileCollection(IJ4JLogger? logger)
        : base(logger)
    {
    }

    protected override void UpdateInternal()
    {
    }

    public override bool TryGetTile(int row, int column, out MultiTileCoordinates? result)
    {
        result = null;
        return false;
    }

    //public IEnumerable<TileCoordinates> GetRelevantTiles( Rect viewPort, IZoom zoom )
    //{
    //    for( var xTile = 0; xTile < zoom.NumTiles; xTile++ )
    //    {
    //        for( var yTile = 0; yTile < zoom.NumTiles; yTile++ )
    //        {
    //            var tilePoint = new IntPoint( xTile, yTile );

    //            var screenPoint = zoom.TileToScreen( tilePoint );

    //            if( screenPoint.X >= viewPort.Left
    //            && screenPoint.X <= viewPort.Right
    //            && screenPoint.Y >= viewPort.Top
    //            && screenPoint.Y <= viewPort.Bottom )
    //                yield return new MultiTileCoordinates( screenPoint,
    //                                                       tilePoint,
    //                                                       zoom.ToLatLong( screenPoint ),
    //                                                       zoom );
    //        }
    //    }
    //}
}