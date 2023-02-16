using J4JSoftware.Logging;

namespace J4JMapLibrary;

public abstract class OpenMapProjection : TiledProjection<string>
{
    private bool _authenticated;

    protected OpenMapProjection(
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( logger, tileCache )
    {
    }

    protected OpenMapProjection(
        IProjectionCredentials credentials,
        IJ4JLogger logger,
        ITileCache? tileCache = null
    )
        : base( credentials, logger, tileCache )
    {
    }

    public override bool Initialized => base.Initialized && _authenticated;

#pragma warning disable CS1998
    public override async Task<bool> AuthenticateAsync( string? credentials, CancellationToken ctx = default )
#pragma warning restore CS1998
    {
        if( MapServer is not OpenMapServer mapServer )
        {
            Logger.Error( "Undefined or inaccessible IMessageCreator, cannot initialize" );
            return false;
        }

        credentials ??= LibraryConfiguration?.Credentials
                                             .Where( x => x.Name.Equals( Name, StringComparison.OrdinalIgnoreCase ) )
                                             .Select( x => x.ApiKey )
                                             .FirstOrDefault();

        if( credentials == null )
        {
            Logger.Error( "No credentials provided or available" );
            return false;
        }

        _authenticated = false;

        mapServer.UserAgent = credentials;
        MapScale.Scale = MapServer.MinScale;

        _authenticated = true;

        return true;
    }
}
