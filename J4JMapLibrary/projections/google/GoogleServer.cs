using System.Security.Cryptography;
using System.Text;

namespace J4JMapLibrary;

[ MapServer( "GoogleMaps", typeof( GoogleCredentials ) ) ]
public class GoogleServer : MapServer<StaticFragment, GoogleCredentials>, IGoogleMapServer
{
    private string _apiKey = string.Empty;
    private string _signature = string.Empty;

    public GoogleServer()
    {
        MinScale = 0;
        MaxScale = 20;
        Copyright = "© Google";
        CopyrightUri = new Uri("http://www.google.com");

        // this doesn't have the required signature field, but that gets appended
        // when the request is created because it involves a cryptographic call
        // against the raw URL
        RetrievalUrl =
            "https://maps.googleapis.com/maps/api/staticmap?center={center}&format={format}&zoom={zoom}&size={size}&key={apikey}";
    }

    public override bool Initialized => !string.IsNullOrEmpty( _apiKey ) && !string.IsNullOrEmpty( _signature );

    public GoogleMapType MapType { get; set; } = GoogleMapType.RoadMap;
    public GoogleImageFormat ImageFormat { get; set; } = GoogleImageFormat.Png;
    public string RetrievalUrl { get; }

#pragma warning disable CS1998
    public override async Task<bool> InitializeAsync( GoogleCredentials credentials, CancellationToken ctx = default )
#pragma warning restore CS1998
    {
        _apiKey = credentials.ApiKey;
        _signature = credentials.SignatureSecret;

        return Initialized;
    }

    public override HttpRequestMessage? CreateMessage( StaticFragment mapFragment, int scale )
    {
        if( !Initialized )
        {
            Logger.Error( "Trying to create image retrieval HttpRequestMessage when uninitialized" );
            return null;
        }

        var replacements = new Dictionary<string, string>
        {
            { "{center}", $"{mapFragment.Center.Latitude}, {mapFragment.Center.Longitude}" },
            { "{format}", ImageFormat.ToString() },
            { "{zoom}", scale.ToString() },
            { "{size}", $"{mapFragment.Width}x{mapFragment.Height}" },
            { "{apikey}", _apiKey }
        };

        var unsignedUrl = ReplaceParameters( RetrievalUrl, replacements );
        var signedUrl = SignUrl( unsignedUrl );

        return new HttpRequestMessage( HttpMethod.Get, new Uri( signedUrl ) );
    }

    public string SignUrl( string url )
    {
        var encoding = new ASCIIEncoding();

        // converting key to bytes will throw an exception, need to replace '-' and '_' characters first.
        var usablePrivateKey = _signature.Replace( "-", "+" )
                                         .Replace( "_", "/" );

        var privateKeyBytes = Convert.FromBase64String( usablePrivateKey );

        var uri = new Uri( url );
        var encodedPathAndQueryBytes = encoding.GetBytes( uri.LocalPath + uri.Query );

        // compute the hash
        var algorithm = new HMACSHA1( privateKeyBytes );
        var hash = algorithm.ComputeHash( encodedPathAndQueryBytes );

        // convert the bytes to string and make url-safe by replacing '+' and '/' characters
        var signature = Convert.ToBase64String( hash )
                               .Replace( "+", "-" )
                               .Replace( "/", "_" );

        // Add the signature to the existing URI.
        return $"{uri.Scheme}://{uri.Host}{uri.LocalPath}{uri.Query}&signature={signature}";
    }
}
