namespace J4JMapLibrary;

public class OpenMapServer : MapServer<TiledFragment, string>
{
    private string _userAgent = string.Empty;

    protected OpenMapServer()
    {
        ImageFileExtension = ".png";
        TileHeightWidth = 256;
    }

    public string RetrievalUrl { get; init; } = string.Empty;
    public override bool Initialized => !string.IsNullOrEmpty( _userAgent );

#pragma warning disable CS1998
    public override async Task<bool> InitializeAsync( string userAgent, CancellationToken ctx = default )
#pragma warning restore CS1998
    {
        _userAgent = userAgent;

        return !string.IsNullOrEmpty( _userAgent );
    }

    public override HttpRequestMessage? CreateMessage( TiledFragment mapFragment, int scale )
    {
        if( !Initialized )
            return null;

        if( string.IsNullOrEmpty( _userAgent ) )
        {
            Logger.Error( "Undefined or empty User-Agent" );
            return null;
        }

        var replacements = new Dictionary<string, string>
        {
            { "{zoom}", scale.ToString()},
            { "{x}", mapFragment.X.ToString() },
            { "{y}", mapFragment.Y.ToString() },
        };

        var uriText = ReplaceParameters( RetrievalUrl, replacements ); 

        var retVal = new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );
        retVal.Headers.Add( "User-Agent", _userAgent );

        return retVal;
    }
}
