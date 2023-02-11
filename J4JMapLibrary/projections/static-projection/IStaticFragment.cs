namespace J4JMapLibrary;

public interface IStaticFragment : IMapFragment
{
    LatLong Center { get; }
    float Height { get; }
    float Width { get; }
}
