using J4JSoftware.Logging;

namespace J4JMapLibrary;

[ Projection( "GoogleMaps" ) ]
public sealed class GoogleMapsProjection : StaticProjection<GoogleCredentials>
{
    private bool _authenticated;

    public GoogleMapsProjection(
        IJ4JLogger logger
    )
        : base( logger )
    {
        MapServer = new GoogleMapsServer();
        MapScale = new ProjectionScale( MapServer );
    }

    public GoogleMapsProjection(
        IProjectionCredentials credentials,
        IJ4JLogger logger
    )
        : base( credentials, logger )
    {
        MapServer = new GoogleMapsServer();
        MapScale = new ProjectionScale(MapServer);
    }

    public override bool Initialized => base.Initialized && _authenticated;

    public override IMapServer MapServer { get; }
    public override IProjectionScale MapScale { get; }

#pragma warning disable CS1998
    public override async Task<bool> AuthenticateAsync( GoogleCredentials? credentials, CancellationToken ctx = default )
#pragma warning restore CS1998
    {
        if (MapServer is not GoogleMapsServer googleServer)
        {
            Logger.Error("Undefined or inaccessible IMessageCreator, cannot initialize");
            return false;
        }

        if( credentials == null )
        {
            if( LibraryConfiguration?
               .Credentials
               .FirstOrDefault( x => x.Name.Equals( Name, StringComparison.OrdinalIgnoreCase ) ) is not SignedCredential
               signedCredential )
            {
                Logger.Error( "Configuration credential not found or is not a SignedCredential" );
                return false;
            }

            credentials = new GoogleCredentials( signedCredential.ApiKey, signedCredential.Signature );
        }

        _authenticated = false;

        googleServer.ApiKey = credentials.ApiKey;
        googleServer.Signature = credentials.SignatureSecret;

        _authenticated = true;

        MapScale.Scale = MapServer.MinScale;

        return true;
    }
}
