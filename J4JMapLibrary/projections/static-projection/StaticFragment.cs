namespace J4JMapLibrary;

public class StaticFragment : MapFragment, IStaticFragment
{
    public StaticFragment(
        IProjection projection,
        float latitude,
        float longitude,
        float height,
        float width,
        int scale
    )
        : base( projection )
    {
        Center = new LatLong( projection.MapServer );
        Center.SetLatLong( latitude, longitude );

        Height = height;
        Width = width;
        Scale = scale;
    }

    protected override string TileId => $"(Lat: {Center.Latitude}, Long: {Center.Longitude})";

    public LatLong Center { get; }
    public float Height { get; }
    public float Width { get; }
    public int Scale { get; }
}
