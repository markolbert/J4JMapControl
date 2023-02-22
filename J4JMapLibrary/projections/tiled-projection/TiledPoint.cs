using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JSoftware.J4JMapLibrary;

public class TiledPoint : StaticPoint
{
    private MinMax<int> _xTileRange = new( 0, 0 );
    private MinMax<int> _yTileRange = new( 0, 0 );

    public TiledPoint(
        ITiledProjection projection
    )
        : base( projection )
    {
        SetTileRanges();
    }

    protected override void OnScaleChanged()
    {
        base.OnScaleChanged();
        SetTileRanges();
    }

    private void SetTileRanges()
    {
        _xTileRange = ( (ITiledProjection) Projection ).TileXRange;
        _yTileRange = ( (ITiledProjection) Projection ).TileYRange;
    }

    public int XTile => X / Projection.MapServer.TileHeightWidth;
    public int YTile => Y / Projection.MapServer.TileHeightWidth;
}
