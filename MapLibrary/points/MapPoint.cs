namespace J4JSoftware.MapLibrary;

public class MapPoint
{
    private bool _ignoreChangeEvents;

    public MapPoint(
        IZoom zoom
    )
    {
        Zoom = zoom;

        AbsolutePixelPoint = new AbsolutePixelPoint();
        AbsolutePixelPoint.ValueChanged += AbsolutePixelPointOnValueChanged;

        if( zoom.MapRetrieverInfo == null )
            throw new ArgumentException(
                $"Attempting to create {typeof( MapPoint )} with an undefined {nameof( MapRetrieverInfo )}" );

        LatLong = new LatLong( zoom.MapRetrieverInfo );
        LatLong.ValueChanged += LatLongOnValueChanged;

        TileRelativePixel = new RelativePixelPoint();
        TileRelativePixel.ValueChanged += TileRelativePixelOnValueChanged;

        Tile = new TilePoint( new IntLimits( 0, zoom.NumTiles - 1 ) );
        Tile.ValueChanged += TileOnValueChanged;
    }

    private MapPoint( MapPoint toCopy )
    {
        LatLong = toCopy.LatLong.Copy();
        LatLong.ValueChanged += LatLongOnValueChanged;

        TileRelativePixel = (RelativePixelPoint) toCopy.TileRelativePixel.Copy();
        TileRelativePixel.ValueChanged += TileRelativePixelOnValueChanged;

        AbsolutePixelPoint = new AbsolutePixelPoint();
        AbsolutePixelPoint.ValueChanged += AbsolutePixelPointOnValueChanged;

        Tile = toCopy.Tile.Copy();
        Tile.ValueChanged += TileOnValueChanged;

        Zoom = toCopy.Zoom;
    }

    public MapPoint Copy() => new( this );

    public IZoom Zoom { get; }

    public LatLong LatLong { get; }

    private void LatLongOnValueChanged( object? sender, EventArgs e )
    {
        if( _ignoreChangeEvents )
            return;

        var newAbsolute = Zoom.GetAbsolutePixelCoordinates( LatLong );
        var newTile = Zoom.AbsolutePointToTile( newAbsolute );

        lock( this )
        {
            _ignoreChangeEvents = true;

            TileRelativePixel.Set( newAbsolute );
            Tile.Set( newTile );

            _ignoreChangeEvents = false;
        }
    }

    public RelativePixelPoint TileRelativePixel { get; }

    private void TileRelativePixelOnValueChanged( object? sender, EventArgs e )
    {
        if( _ignoreChangeEvents )
            return;

        var screenPt = new DoublePoint( TileRelativePixel.X, TileRelativePixel.Y );
        var newLatLong = Zoom.RelativePointToLatLong( screenPt );
        var newTile = Zoom.AbsolutePointToTile( screenPt );

        lock( this )
        {
            _ignoreChangeEvents = true;

            LatLong.Set( newLatLong );
            Tile.Set( newTile );

            _ignoreChangeEvents = false;
        }
    }

    public AbsolutePixelPoint AbsolutePixelPoint { get; }

    private void AbsolutePixelPointOnValueChanged(object? sender, EventArgs e)
    {
        if (_ignoreChangeEvents)
            return;

        var tilePt = new IntPoint(Tile.X, Tile.Y);
        var newScreenPt = Zoom.TileToAbsolutePoint(tilePt);
        var newLatLong = Zoom.RelativePointToLatLong(newScreenPt);

        lock (this)
        {
            _ignoreChangeEvents = true;

            LatLong.Set(newLatLong);
            TileRelativePixel.Set(newScreenPt);

            _ignoreChangeEvents = false;
        }
    }

    public TilePoint Tile { get; }

    private void TileOnValueChanged( object? sender, EventArgs e )
    {
        if( _ignoreChangeEvents )
            return;

        var tilePt = new IntPoint( Tile.X, Tile.Y );
        var newScreenPt = Zoom.TileToAbsolutePoint( tilePt );
        var newLatLong = Zoom.RelativePointToLatLong( newScreenPt );

        lock( this )
        {
            _ignoreChangeEvents = true;

            LatLong.Set( newLatLong );
            TileRelativePixel.Set( newScreenPt );

            _ignoreChangeEvents = false;
        }
    }

    public MapPoint OffsetByPixel( double xOffset, double yOffset )
    {
        var retVal = Copy();

        var newScreenPt = new DoublePoint(retVal.TileRelativePixel.X + xOffset, retVal.TileRelativePixel.Y + yOffset);
        retVal.TileRelativePixel.Set( newScreenPt );

        return retVal;
    }
}
