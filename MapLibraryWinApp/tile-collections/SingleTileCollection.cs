using System;
using System.Collections.Generic;
using Windows.Foundation;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public class SingleTileCollection : ITileCollection
{
    public IEnumerable<TileCoordinates> GetRelevantTiles( Rect viewPort, IZoom zoom )
    {
        var upperLeftScreenPt = new IntPoint( RoundToInt( viewPort.Left ), RoundToInt( viewPort.Top ) );
        var lowerRightScreenPt = new IntPoint( RoundToInt( viewPort.Right ), RoundToInt( viewPort.Bottom ) );

        var upperLeftLatLong = zoom.ToLatLong( upperLeftScreenPt );
        var lowerRightLatLong = zoom.ToLatLong( lowerRightScreenPt );

        yield return new SingleTileCoordinates( upperLeftScreenPt,
                                                lowerRightScreenPt,
                                                upperLeftLatLong,
                                                lowerRightLatLong,
                                                zoom );
    }

    private int RoundToInt( double toRound ) => Convert.ToInt32( Math.Round( toRound ) );
}
