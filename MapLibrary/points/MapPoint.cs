namespace J4JSoftware.MapLibrary;

public class MapPoint
{
    private bool _ignoreChangeEvents;

    public MapPoint(
        IZoom zoom
    )
    {
        Zoom = zoom;

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

    public IZoom Zoom { get; }

    public LatLong LatLong { get; }

    private void LatLongOnValueChanged( object? sender, EventArgs e )
    {
        if( _ignoreChangeEvents )
            return;

        var newScreen = Zoom.GetScreenCoordinates( LatLong );
        var newTile = Zoom.ScreenToTile( newScreen );

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
        var newLatLong = Zoom.ScreenToLatLong( screenPt );
        var newTile = Zoom.ScreenToTile( screenPt );

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
        var newScreenPt = Zoom.TileToScreen( tilePt );
        var newLatLong = Zoom.ScreenToLatLong( newScreenPt );

        lock( this )
        {
            _ignoreChangeEvents = true;

            LatLong.Set( newLatLong );
            Screen.Set( newScreenPt );

            _ignoreChangeEvents = false;
        }
    }
}
