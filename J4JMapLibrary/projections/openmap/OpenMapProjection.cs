using J4JSoftware.Logging;

namespace J4JMapLibrary;

public class OpenMapProjection : TiledProjection<string>
{
    private bool _authenticated;

    protected OpenMapProjection(
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( mapServer, new TiledScale(mapServer), logger, tileCache )
    {
    }

    protected OpenMapProjection(
        IProjectionCredentials credentials,
        IMapServer mapServer,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( credentials, mapServer, new TiledScale(mapServer), logger, tileCache )
    {
    }

    public override bool Initialized => base.Initialized && _authenticated;

    public override async Task<bool> AuthenticateAsync( string? credentials, CancellationToken ctx = default )
    {
        MapScale.Scale = MapServer.MinScale;

        credentials ??= LibraryConfiguration?.Credentials
                                             .Where( x => x.Name.Equals( Name, StringComparison.OrdinalIgnoreCase ) )
                                             .Select( x => x.ApiKey )
                                             .FirstOrDefault();

        if( credentials == null )
        {
            Logger.Error( "No credentials provided or available" );
            return false;
        }

        if( MapServer is not IOpenMapServer mapServer )
        {
            Logger.Error( "Undefined or inaccessible IMessageCreator, cannot initialize" );
            return false;
        }

        _authenticated = false;

        if( !await mapServer.InitializeAsync( credentials, ctx ) )
            return false;

        MapScale.Scale = MapServer.MinScale;

        _authenticated = true;

        return true;
    }
}
