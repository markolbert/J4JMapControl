namespace J4JSoftware.MapLibrary;

public class MapPoint
{
    private readonly IZoom _zoom;

    private bool _ignoreChangeEvents;

    public MapPoint(
        IZoom zoom
    )
    {
        _zoom = zoom;

        if( zoom.MapRetrieverInfo == null )
            throw new ArgumentException(
                $"Attempting to create {typeof( MapPoint )} with an undefined {nameof( MapRetrieverInfo )}" );

        LatLong = new LatLong( zoom.MapRetrieverInfo );
        LatLong.ValueChanged += LatLongOnValueChanged;

        Screen = new ScreenPoint();
        Screen.ValueChanged += ScreenOnValueChanged;

        Tile = new TilePoint( zoom );
        Tile.ValueChanged += TileOnValueChanged;
    }

    public LatLong LatLong { get; }

    private void LatLongOnValueChanged( object? sender, EventArgs e )
    {
        if( _ignoreChangeEvents )
            return;

        var newScreen = _zoom.GetScreenCoordinates( LatLong );
        var newTile = _zoom.ScreenToTile( newScreen );

        lock( this )
        {
            _ignoreChangeEvents = true;

            Screen.Set( newScreen );
            Tile.Set( newTile );

            _ignoreChangeEvents = false;
        }
    }

    public ScreenPoint Screen { get; }

    private void ScreenOnValueChanged( object? sender, EventArgs e )
    {
        if( _ignoreChangeEvents )
            return;

        var screenPt = new DoublePoint( Screen.X, Screen.Y );
        var newLatLong = _zoom.ToLatLong( screenPt );
        var newTile = _zoom.ScreenToTile( screenPt );

        lock( this )
        {
            _ignoreChangeEvents = true;

            LatLong.Set( newLatLong );
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

            LatLong.Set( newLatLong );
            Screen.Set( newScreenPt );

            _ignoreChangeEvents = false;
        }
    }
}
