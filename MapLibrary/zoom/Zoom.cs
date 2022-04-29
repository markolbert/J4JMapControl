using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.MapLibrary;

public class Zoom : IZoom
{
    public event EventHandler<int>? Changed;
    
    private int _level;
    private IJ4JLogger? _logger;

    public Zoom( IMapContext mapContext )
    {
        Initialize(mapContext);
    }

    public Zoom( 
        MapRetrieverInfo mapRetrieverInfo
        )
    {
        MapRetrieverInfo = mapRetrieverInfo;
        Initialize();
    }

    public Zoom(
        int level,
        MapRetrieverInfo mapRetrieverInfo
        )
    {
        MapRetrieverInfo = mapRetrieverInfo;
        Initialize();

        Level = level;
    }

    private void Initialize( IMapContext mapContext )
    {
        MapRetrieverInfo = mapContext.MapRetriever.MapRetrieverInfo;

        if( MapRetrieverInfo != null )
            Initialize();

        var msg =
            $"Attempting to create instance of {nameof(Zoom)} from an undefined {nameof(MapLibrary.MapRetrieverInfo)}";

        J4JDeusEx.Logger?.Fatal(msg);

        throw new J4JDeusExException(msg);
    }

    private void Initialize()
    {
        _logger = J4JDeusEx.ServiceProvider.GetRequiredService<IJ4JLogger>();
        _logger?.SetLoggedType(GetType());

        Maximum = MapRetrieverInfo!.MaximumZoom;
        Minimum = MapRetrieverInfo.MinimumZoom;
        _level = MapRetrieverInfo.MinimumZoom;

        RetrievalBitmapWidthHeight = MapRetrieverInfo.DefaultBitmapWidthHeight;

        WidthHeight = RetrievalBitmapWidthHeight * 2 ^ Level;
        NumTiles = 2 ^ Level;

        Changed?.Invoke( this, Level );
    }

    public MapRetrieverInfo? MapRetrieverInfo { get; private set; }

    public int Level
    {
        get => GetLimitedValue( _level );

        set
        {
            var limitedValue = GetLimitedValue( value );

            _level = value;

            if( limitedValue != value )
                Changed?.Invoke( this, _level );
        }
    }

    private int GetLimitedValue( int value ) =>
        value < Minimum
            ? Minimum
            : value > Maximum
                ? Maximum
                : value;

    public int Maximum { get; private set; }
    public int Minimum { get; private set; }

    public int WidthHeight { get; private set; }
    public int RetrievalBitmapWidthHeight { get; private set; }
    public int NumTiles { get; private set; }

    public LatLong ToLatLong( IntPoint screenPoint )
    {
        var adjX = Clip( screenPoint.X, 0, WidthHeight - 1 ) / WidthHeight - 0.5;
        var adjY = 0.5 - Clip( screenPoint.Y, 0, WidthHeight - 1 ) / WidthHeight;

        var retVal = new LatLong( MapRetrieverInfo! );
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
}
