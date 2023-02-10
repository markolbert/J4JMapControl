namespace J4JMapLibrary;

public class StaticFragment : MapFragment<TiledScope>, IStaticFragment
{
    public StaticFragment(
        IMapProjection projection,
        float latitude,
        float longitude,
        float height,
        float width,
        int scale
    )
        : base( projection )
    {
        Center = new LatLong( Scope );
        Center.SetLatLong( latitude, longitude );

        Height = height;
        Width = width;

        Scale = Scope.ScaleRange.ConformValueToRange( scale, "Scale" );
    }

    protected override string TileId => $"(Lat: {Center.Latitude}, Long: {Center.Longitude}, Scale: {Scale})";

    public LatLong Center { get; }
    public float Height { get; }
    public float Width { get; }
    public int Scale { get; }
}
