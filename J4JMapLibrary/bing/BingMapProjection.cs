using J4JSoftware.Logging;
using System.Net;
using System.Text.Json;

namespace J4JMapLibrary;

public class BingMapProjection : MapProjection
{
    private const string MetadataUrlTemplate =
        "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/Mode?output=json&key=ApiKey";

    private readonly string _apiKey;

    public BingMapProjection(
        BingMapType mapType,
        string apiKey,
        IJ4JLogger logger
    )
        : base( logger )
    {
        MapType = mapType;
        _apiKey = apiKey;
    }

    public BingMapType MapType { get; }
    public BingImageryMetadata? Metadata { get; private set; }

    public override double MinLatitude => Metadata?.PrimaryResource?.

    public override async Task<bool> InitializeAsync()
    {
        var uri = new Uri( MetadataUrlTemplate.Replace( "Mode", MapType.ToString() )
                                              .Replace( "ApiKey", _apiKey ) );

        var request = new HttpRequestMessage( HttpMethod.Get, uri );

        var uriText = uri.AbsoluteUri;
        var httpClient = new HttpClient();

        HttpResponseMessage? response = null;

        Logger.Verbose( "Attempting to retrieve Bing Maps metadata" );
        try
        {
            response = await httpClient.SendAsync( request );
        }
        catch( Exception ex )
        {
            Logger.Error<string, string>( "Could not retrieve Bing Maps Metadata from {0}, message was '{1}'",
                                          uriText,
                                          ex.Message );

            return false;
        }

        if( response.StatusCode != HttpStatusCode.OK )
        {
            var error = await response.Content.ReadAsStringAsync();

            Logger.Error<string, string>(
                "Invalid response code received from {0} when retrieving Bing Maps Metadata, message was '{1}'",
                uriText,
                error );

            return false;
        }

        Logger.Verbose( "Attempting to parse Bing Maps metadata" );
        try
        {
            var respText = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            Metadata = JsonSerializer.Deserialize<BingImageryMetadata>( respText, options );
            Logger.Verbose( "Bing Maps metadata retrieved" );
        }
        catch( Exception ex )
        {
            Logger.Error<string>( "Could not parse Bing Maps metadata, message was '{0}'", ex.Message );

            return false;
        }

        Initialized = true;

        return true;
    }
    
    public override (double latitude, double longitude) ConvertToLatLong( int x, int y ) =>
        throw new NotImplementedException();

    public override (int x, int y) ConvertToXY( double latitude, double longitude ) =>
        throw new NotImplementedException();
}
