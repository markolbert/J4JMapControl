using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class SimpleMapFragments : MapFragments<IMapFragment>
{
    private static IMapFragment DefaultFactory( IMapFragment fragment ) => fragment;

    public SimpleMapFragments(
        IProjection projection,
        IJ4JLogger logger
    )
        : base( projection, DefaultFactory, logger )
    {
    }
}
