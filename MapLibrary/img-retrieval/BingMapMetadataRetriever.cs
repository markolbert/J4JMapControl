using System;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace J4JSoftware.MapLibrary
{
    internal static class BingMapMetadataRetriever
    {
        private const string MetadataUrlTemplate =
            "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/Mode?output=json&key=ApiKey";

        public static async Task<AsyncWebResult<BingMetadata.ImageryMetadata>> GetBingMetadata(
            BingMapType mapType,
            string apiKey
        )
        {
            var uri = new Uri( MetadataUrlTemplate.Replace( "Mode", mapType.ToString() )
                                                  .Replace( "ApiKey", apiKey ) );

            var request = new HttpRequestMessage( HttpMethod.Get, uri );

            var uriText = uri.AbsoluteUri;
            var httpClient = new HttpClient();

            HttpResponseMessage? response = null;

            try
            {
                response = await httpClient.SendRequestAsync( request );
            }
            catch( Exception ex )
            {
                return new AsyncWebResult<BingMetadata.ImageryMetadata>( null,
                    (int) (response?.StatusCode ?? HttpStatusCode.BadRequest),
                    $"Metadata request from {uriText} failed, message was '{ex.Message}'" );
            }

            if( response.StatusCode != HttpStatusCode.Ok )
            {
                var error = await response.Content.ReadAsStringAsync();

                return new AsyncWebResult<BingMetadata.ImageryMetadata>( null,
                    (int) response.StatusCode,
                    $"Invalid response code from {uriText} ({response.StatusCode}), message was '{error}'" );
            }

            try
            {
                var respText = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

                var retVal = JsonSerializer.Deserialize<BingMetadata.ImageryMetadata>( respText, options );

                return new AsyncWebResult<BingMetadata.ImageryMetadata>( retVal,
                    (int) response.StatusCode,
                    request.RequestUri.AbsoluteUri );
            }
            catch( Exception ex )
            {
                return new AsyncWebResult<BingMetadata.ImageryMetadata>( null,
                    (int) response.StatusCode,
                    request.RequestUri.AbsoluteUri,
                    $"Could not retrieve metadata, message was '{ex.Message}'" );
            }
        }
    }
}
