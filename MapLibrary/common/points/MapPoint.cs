namespace J4JSoftware.MapLibrary;

public class MapPoint
{
    private readonly IZoom _zoom;

    private bool _ignoreChangeEvents;

    public MapPoint(
        IZoom zoom,
        IMapSourceParameters? mapSource = null
    )
    {
        _zoom = zoom;

        UpperLeftLatLong = new LatLong();
        UpperLeftLatLong.ValueChanged += UpperLeftLatLongOnValueChanged;

        Screen = new ScreenPoint();
        Screen.ValueChanged += ScreenOnValueChanged;

        Tile = new TilePoint( zoom );
        Tile.ValueChanged += TileOnValueChanged;

        LowerRightLatLong = new LatLong();
        LowerRightLatLong.Set( GetLowerRightLatLong() );
    }

    public LatLong UpperLeftLatLong { get; }

    private void UpperLeftLatLongOnValueChanged( object? sender, EventArgs e )
    {
        if( _ignoreChangeEvents )
            return;

        var newScreen = _zoom.GetScreenCoordinates( UpperLeftLatLong );
        var newTile = _zoom.ScreenToTile( newScreen );

        lock( this )
        {
            _ignoreChangeEvents = true;

            Screen.Set( newScreen );
            Tile.Set( newTile );

            _ignoreChangeEvents = false;
        }

        LowerRightLatLong.Set( GetLowerRightLatLong() );
    }

    public LatLong LowerRightLatLong { get; }

    private DoublePoint GetLowerRightLatLong()
    {
        return DoublePoint.Empty;
    }

    public ScreenPoint Screen { get; }

    private void ScreenOnValueChanged( object? sender, EventArgs e )
    {
        if( _ignoreChangeEvents )
            return;

        var screenPt = new IntPoint( Screen.X, Screen.Y );
        var newLatLong = _zoom.ToLatLong( screenPt );
        var newTile = _zoom.ScreenToTile( screenPt );

        lock( this )
        {
            _ignoreChangeEvents = true;

            UpperLeftLatLong.Set( newLatLong );
            Tile.Set( newTile );

            _ignoreChangeEvents = false;
        }
    }

    public TilePoint Tile { get; }

    private void TileOnValueChanged( object? sender, EventArgs e )
    {
        if( _ignoreChangeEvents )
            return;

        var tilePt = new IntPoint( Tile.X, Tile.Y );
        var newScreenPt = _zoom.TileToScreen( tilePt );
        var newLatLong = _zoom.ToLatLong( newScreenPt );

        lock( this )
        {
            _ignoreChangeEvents = true;

            UpperLeftLatLong.Set( newLatLong );
            Screen.Set( newScreenPt );

            _ignoreChangeEvents = false;
        }
    }
}
