namespace J4JSoftware.MapLibrary;

public abstract record Zoom : IZoom
{
    private readonly IMapSourceParameters _mapSrcParams;

    protected Zoom(
        int level,
        int maxLevel,
        IMapSourceParameters mapSrcParams
    )
    {
        _mapSrcParams = mapSrcParams;

        MaxLevel = maxLevel <= 1 ? GlobalConstants.MaxDetailLevel : maxLevel;

        if( level <= 1 )
            level = 1;

        Level = level > MaxLevel ? MaxLevel : level;
        WidthHeight = 256 * 2 ^ Level;
        NumTiles = 2 ^ Level;
    }

    public int Level { get; }
    public int MaxLevel { get; }

    public int WidthHeight { get; }
    public int NumTiles { get; }

    public LatLong ToLatLong( IntPoint screenPoint )
    {
        var adjX = Clip( screenPoint.X, 0, WidthHeight - 1 ) / WidthHeight - 0.5;
        var adjY = 0.5 - Clip( screenPoint.Y, 0, WidthHeight - 1 ) / WidthHeight;

        return new LatLong( 90 - 360 * Math.Atan( Math.Exp( -adjY * 2 * Math.PI ) ) / Math.PI,
                            360 * adjX,
                            _mapSrcParams );
    }

    public IntPoint LatLongToScreen( LatLong latLong )
    {
        var x = ( latLong.Longitude + 180 ) / 360;
        var sinLatitude = Math.Sin( latLong.Latitude * Math.PI / 180 );
        var y = 0.5 - Math.Log( ( 1 + sinLatitude ) / ( 1 - sinLatitude ) ) / ( 4 * Math.PI );

        return new IntPoint( (int) Clip( x * WidthHeight + 0.5, 0, WidthHeight - 1 ),
                             (int) Clip( y * WidthHeight + 0.5, 0, WidthHeight - 1 ) );
    }

    private double Clip(double n, double minValue, double maxValue) =>
        Math.Min(Math.Max(n, minValue), maxValue);

    public abstract IntPoint TileToScreen( IntPoint tilePoint );
    public abstract IntPoint ScreenToTile( IntPoint screenPoint );
}
