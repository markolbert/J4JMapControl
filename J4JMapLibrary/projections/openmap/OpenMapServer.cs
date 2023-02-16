namespace J4JMapLibrary;

public class OpenMapServer : MapServer<TiledFragment, string>
{
    protected OpenMapServer()
    {
        ImageFileExtension = ".png";
        TileHeightWidth = 256;
    }

    public string RetrievalUrl { get; init; } = string.Empty;
    public override bool Initialized => !string.IsNullOrEmpty( UserAgent );
    public string UserAgent { get; internal set; } = string.Empty;

    public override HttpRequestMessage? CreateMessage( TiledFragment mapFragment, int scale )
    {
        if( !Initialized )
            return null;

        if( string.IsNullOrEmpty( UserAgent ) )
        {
            Logger.Error( "Undefined or empty User-Agent" );
            return null;
        }

        var replacements = new Dictionary<string, string>
        {
            { "{zoom}", scale.ToString() }, { "{x}", mapFragment.X.ToString() }, { "{y}", mapFragment.Y.ToString() }
        };

        var uriText = InternalExtensions.ReplaceParameters( RetrievalUrl, replacements );

        var retVal = new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );
        retVal.Headers.Add( "User-Agent", UserAgent );

        return retVal;
    }
}
