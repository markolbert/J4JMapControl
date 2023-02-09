using System.Net;
using System.Text.Json;

namespace J4JMapLibrary;

[ MapServer( "Google", typeof( GoogleCredentials ) ) ]
public class GoogleServer : MapServer<VariableMapTile, GoogleCredentials>, IGoogleMapServer
{
    private string _apiKey = string.Empty;
    private string _signature = string.Empty;

    public GoogleServer()
    {
        MinScale = 0;
        MaxScale = 20;
        Copyright = "© Google";
        CopyrightUri = new Uri("http://www.google.com");
        RetrievalUrl =
            "https://maps.googleapis.com/maps/api/staticmap?center={center}&format={format}&zoom={zoom}&size={size}&key={apikey}&signature={signature}";
    }

    public override bool Initialized => !string.IsNullOrEmpty( _apiKey ) && !string.IsNullOrEmpty( _signature );

    public GoogleMapType MapType { get; private set; } = GoogleMapType.RoadMap;
    public GoogleImageFormat ImageFormat { get; set; } = GoogleImageFormat.Png;
    public string RetrievalUrl { get; }

#pragma warning disable CS1998
    public override async Task<bool> InitializeAsync( GoogleCredentials credentials, CancellationToken ctx = default )
#pragma warning restore CS1998
    {
        _apiKey = credentials.ApiKey;
        _signature = credentials.Signature;

        return Initialized;
    }

    public override HttpRequestMessage? CreateMessage( VariableMapTile tile )
    {
        if( !Initialized )
        {
            Logger.Error( "Trying to create image retrieval HttpRequestMessage when uninitialized" );
            return null;
        }

        var replacements = new Dictionary<string, string>
        {
            { "{center}", $"{tile.Center.Latitude}, {tile.Center.Longitude}" },
            { "{format}", ImageFormat.ToString() },
            { "{zoom}", tile.Scale.ToString() },
            { "{size}", $"{tile.Width}x{tile.Height}" },
            { "{apikey}", _apiKey },
            { "{signature}", _signature }
        };

        var uriText = ReplaceParameters( RetrievalUrl, replacements );

        return new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );
    }
}
