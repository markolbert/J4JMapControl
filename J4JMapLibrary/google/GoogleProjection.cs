using J4JSoftware.Logging;
using System.Numerics;

namespace J4JMapLibrary;

[ MapProjection( "Google", typeof( IGoogleMapServer ) ) ]
public sealed class GoogleProjection : VariableTileProjection<TileScope, GoogleCredentials>
{
    // "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/Mode?output=json&key=ApiKey";

    private bool _authenticated;

    public GoogleProjection(
        IGoogleMapServer mapServer,
        IJ4JLogger logger
    )
        : base( mapServer, logger )
    {
        SetSizes( 1 );
    }

    public GoogleProjection(
        IProjectionCredentials credentials,
        IGoogleMapServer mapServer,
        IJ4JLogger logger
    )
        : base( credentials, mapServer, logger )
    {
        SetSizes( 1 );
    }

    public override bool Initialized => base.Initialized && _authenticated;

    public override async Task<bool> AuthenticateAsync( GoogleCredentials? credentials, CancellationToken ctx = default )
    {
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

        SetScale( Scope.ScaleRange.Minimum );

        return true;
    }
}
