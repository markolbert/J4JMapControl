using J4JSoftware.Logging;

namespace J4JMapLibrary;

[ Projection( "GoogleMaps", typeof( IGoogleMapServer ) ) ]
public sealed class GoogleMapsProjection : StaticProjection<GoogleCredentials>
{
    // "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/Mode?output=json&key=ApiKey";

    private bool _authenticated;

    public GoogleMapsProjection(
        IGoogleMapServer mapServer,
        IJ4JLogger logger
    )
        : base( mapServer, new ProjectionScale(mapServer), logger )
    {
    }

    public GoogleMapsProjection(
        IProjectionCredentials credentials,
        IGoogleMapServer mapServer,
        IJ4JLogger logger
    )
        : base( credentials, mapServer, new ProjectionScale(mapServer), logger )
    {
    }

    public override bool Initialized => base.Initialized && _authenticated;

    public override async Task<bool> AuthenticateAsync( GoogleCredentials? credentials, CancellationToken ctx = default )
    {
        MapScale.Scale = MapServer.MinScale;

        if (MapServer is not IGoogleMapServer googleServer)
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

        var initialized = MapServer.MaxRequestLatency < 0
            ? await googleServer.InitializeAsync( credentials, ctx )
            : await googleServer.InitializeAsync( credentials, ctx )
                              .WaitAsync( TimeSpan.FromMilliseconds( MapServer.MaxRequestLatency ), ctx );

        if( !initialized )
            return false;

        _authenticated = true;

        MapScale.Scale = MapServer.MinScale;

        return true;
    }
}
