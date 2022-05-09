using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public class BingMapsImageRetriever : TileBasedImageRetriever
{
    private static readonly AutoResetEvent AutoReset = new(false);

    private readonly string _apiKey;

    private string? _cultureCode;

    public BingMapsImageRetriever(
        string apiKey,
        BingMapType mapType,
        IJ4JLogger? logger
    )
        : base( logger )
    {
        _apiKey = apiKey;
        MapType = mapType;
    }

    public BingMapType MapType { get; }
    public BingMetadata.ImageryMetadata? Metadata { get; private set; }

    protected override MapRetrieverInfo GetMapRetrieverInfo( IMapProjection mapProjection )
    {
        AutoReset.Reset();

        BingMapRetrieverInfo? mdInfo = null;

        Task.Run(async () =>
        {
            mdInfo = await GetMetadata();
            AutoReset.Set();
        });

        AutoReset.WaitOne(3000);

        if( mdInfo != null)
            return mdInfo;

        Logger?.Fatal("Could not retrieve Bing metadata info");

        throw new InvalidOperationException( "Could not retrieve Bing metadata info" );
    }

    public bool SetCultureCode( string code )
    {
        if( !BingMapsCultureCodes.Default.ContainsKey( code ) )
        {
            Logger?.Error<string>( "Invalid or unsupported culture code '{0}'", code );
            return false;
        }

        _cultureCode = code;

        return true;
    }

    protected override HttpRequestMessage GetRequest( TileCoordinates tile )
    {
        var subDomain = ( (BingMapRetrieverInfo) MapRetrieverInfo ).GetRandomSubdomain();
        var quadKey = tile.GetBingMapsQuadKey();

        var uriText = MapRetrieverInfo.RetrievalUrl.Replace( "{subdomain}", subDomain )
                                      .Replace( "{quadkey}", quadKey )
                                      .Replace( "{culture}", _cultureCode );

        return new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );
    }

    private async Task<BingMapRetrieverInfo?> GetMetadata()
    {
        var temp = await BingMapMetadataRetriever.GetBingMetadata( MapType, _apiKey );

        if( !temp.IsValid )
        {
            Logger?.Error<string?, HttpStatusCode, string?>(
                "Could not get Bing metadata from {0} (status code {1}), message was {2}",
                temp.Url,
                temp.HttpStatusCode,
                temp.Message );

            return null;
        }

        if( !temp.ReturnValue?.IsValid ?? true )
        {
            Logger?.Error( "Bing metadata does not include required Resource object" );
            return null;
        }

        Metadata = temp.ReturnValue!;
        var resource = Metadata.PrimaryResource!;

        if( resource.ImageHeight != resource.ImageWidth )
        {
            Logger?.Error( "Bing metadata indicates non-square image dimensions, which is not supported" );
            return null;
        }

        return new BingMapRetrieverInfo( resource.ImageUrl,
                                         resource.ImageUrlSubdomains.ToList(),
                                         MapType.GetDescription(),
                                         Metadata.Copyright,
                                         new Uri( Metadata.BrandLogoUri ),
                                         GlobalConstants.Wgs84MaxLatitude,
                                         180,
                                         resource.ZoomMin,
                                         resource.ZoomMax,
                                         resource.ImageHeight );
    }
}