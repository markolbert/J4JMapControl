namespace J4JMapLibrary;

public class FixedTileViewport
{
    private float _heading;

    public float CenterLatitude { get; set; }
    public float CenterLongitude { get; set; }
    public float Height { get; set; }
    public float Width { get; set; }

    // in degrees; north is 0/360; stored as mod 360
    public float Heading
    {
        get => _heading;
        set => _heading = value % 360;
    }

    public FixedTileViewport Constrain( TileScope scope ) =>
        new FixedTileViewport
        {
            CenterLatitude = scope.LatitudeRange.ConformValueToRange( CenterLatitude, "Latitude" ),
            CenterLongitude = scope.LongitudeRange.ConformValueToRange( CenterLongitude, "Longitude" ),
            Height = Height,
            Width = Width,
            Heading = Heading
        };
}
