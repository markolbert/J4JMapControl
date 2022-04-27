using J4JSoftware.DeusEx;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.MapLibrary;

public record Zoom : IZoom
{
    public Zoom()
    {
        var mapContext = J4JDeusEx.ServiceProvider.GetRequiredService<IMapContext>();
        Initialize( mapContext.MapImageRetriever.MapRetrieverInfo );

        Level = MinLevel;
    }

    public Zoom( 
        MapRetrieverInfo mapRetrieverInfo
        )
    {
        Initialize(mapRetrieverInfo);
        Level = MinLevel;
    }

    public Zoom(
        int level
        )
    {
        var mapContext = J4JDeusEx.ServiceProvider.GetRequiredService<IMapContext>();
        Initialize(mapContext.MapImageRetriever.MapRetrieverInfo);

        Level = level <= MinLevel ? MinLevel : level;
    }

    private void Initialize( MapRetrieverInfo mapRetrieverInfo )
    {
        MaxLevel = mapRetrieverInfo.MaximumZoom;
        MinLevel = mapRetrieverInfo.MinimumZoom;

        RetrievalBitmapWidthHeight = mapRetrieverInfo.DefaultBitmapWidthHeight;

        WidthHeight = RetrievalBitmapWidthHeight * 2 ^ Level;
        NumTiles = 2 ^ Level;
    }

    public int Level { get; }
    public int MaxLevel { get; private set; }
    public int MinLevel { get; private set; }

    public int WidthHeight { get; private set; }
    public int RetrievalBitmapWidthHeight { get; private set; }
    public int NumTiles { get; private set; }

    public LatLong ToLatLong( IntPoint screenPoint )
    {
        var adjX = Clip( screenPoint.X, 0, WidthHeight - 1 ) / WidthHeight - 0.5;
        var adjY = 0.5 - Clip( screenPoint.Y, 0, WidthHeight - 1 ) / WidthHeight;

        var retVal = new LatLong();
        retVal.Set( new DoublePoint( 90 - 360 * Math.Atan( Math.Exp( -adjY * 2 * Math.PI ) ) / Math.PI, 360 * adjX ) );

        return retVal;
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

    public IntPoint ScreenToTile(IntPoint screenPoint) =>
        new(screenPoint.X / RetrievalBitmapWidthHeight, screenPoint.Y / RetrievalBitmapWidthHeight);

    public IntPoint TileToScreen(IntPoint tilePoint) => new(tilePoint.X * RetrievalBitmapWidthHeight, tilePoint.Y * RetrievalBitmapWidthHeight);

    public int Minimum { get; set; }
    public int Maximum => RetrievalBitmapWidthHeight;
}
