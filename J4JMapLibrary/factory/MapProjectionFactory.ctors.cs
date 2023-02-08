using J4JSoftware.Logging;

namespace J4JMapLibrary;

public partial class MapProjectionFactory
{
    public MapProjectionFactory(
        IProjectionCredentials projCredentials,
        IJ4JLogger logger
    )
        : this( logger )
    {
        _projCredentials = projCredentials;
    }

    public MapProjectionFactory(
        IJ4JLogger logger
    )
    {
        _logger = logger;
        _logger.SetLoggedType( GetType() );
    }
}
