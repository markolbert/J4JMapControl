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

        Pixel = new PixelPoint();
        Pixel.ValueChanged += PixelOnValueChanged;

        Tile = new TilePoint( new IntLimits( 0, zoom.NumTiles - 1 ) );
        Tile.ValueChanged += TileOnValueChanged;
    }

    private MapPoint(
        MapPoint toCopy
    )
    {
        Zoom = toCopy.Zoom;
        LatLong = toCopy.LatLong;
        Pixel = toCopy.Pixel;
        Tile = toCopy.Tile;
    }

    public MapPoint Copy() => new( this );

    public IZoom Zoom { get; }

    public LatLong LatLong { get; }

    private void LatLongOnValueChanged( object? sender, EventArgs e )
    {
        if( _ignoreChangeEvents )
            return;

        var newScreen = Zoom.GetPixelCoordinates( LatLong );
        var newTile = Zoom.PixelToTile( newScreen );

        lock( this )
        {
            _ignoreChangeEvents = true;

            Pixel.Set( newScreen );
            Tile.Set( newTile );

            _ignoreChangeEvents = false;
        }
    }

    public PixelPoint Pixel { get; }

    private void PixelOnValueChanged( object? sender, EventArgs e )
    {
        if( _ignoreChangeEvents )
            return;

        var screenPt = new DoublePoint( Pixel.X, Pixel.Y );
        var newLatLong = Zoom.PixelToLatLong( screenPt );
        var newTile = Zoom.PixelToTile( screenPt );

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
        var newScreenPt = Zoom.TileToPixel( tilePt );
        var newLatLong = Zoom.PixelToLatLong( newScreenPt );

        lock( this )
        {
            _ignoreChangeEvents = true;

            LatLong.Set( newLatLong );
            Pixel.Set( newScreenPt );

            _ignoreChangeEvents = false;
        }
    }

    public MapPoint OffsetByPixel( double xOffset, double yOffset )
    {
        var retVal = this.Copy();

        var newScreenPt = new DoublePoint(retVal.Pixel.X + xOffset, retVal.Pixel.Y + yOffset);
        retVal.Pixel.Set( newScreenPt );

        return retVal;
    }
}
