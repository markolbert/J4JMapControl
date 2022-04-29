using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Media.Imaging;

namespace J4JSoftware.J4JMapControl;

public class BingMapsImageRetriever : ImageDirectImageRetriever<MultiTileCoordinates>
{
    private static readonly AutoResetEvent AutoReset = new AutoResetEvent(false);

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

    public override MapRetrieverInfo? MapRetrieverInfo
    {
        get
        {
            if( base.MapRetrieverInfo != null )
                return base.MapRetrieverInfo;

            AutoReset.Reset();

            Task.Run( async () =>
            {
                await GetMetadata();
                AutoReset.Set();
            } );

            AutoReset.WaitOne( 3000 );

            return base.MapRetrieverInfo;
        }
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

    protected override HttpRequestMessage? GetRequest( MultiTileCoordinates tile )
    {
        if( MapRetrieverInfo == null )
            return null;

        var subDomain = ( (BingMapRetrieverInfo) MapRetrieverInfo ).GetRandomSubdomain();
        var quadKey = tile.GetBingMapsQuadKey();

        var uriText = MapRetrieverInfo!.RetrievalUrl.Replace( "{subdomain}", subDomain )
                                       .Replace( "{quadkey}", quadKey )
                                       .Replace( "{culture}", _cultureCode );

        return new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );
    }

    private async Task<bool> GetMetadata()
    {
        var temp = await BingMapMetadataRetriever.GetBingMetadata( MapType, _apiKey );

        if( !temp.IsValid )
        {
            Logger?.Error<string?, HttpStatusCode, string?>(
                "Could not get Bing metadata from {0} (status code {1}), message was {2}",
                temp.Url,
                temp.HttpStatusCode,
                temp.Message );

            return false;
        }

        if( !temp.ReturnValue?.IsValid ?? true )
        {
            Logger?.Error( "Bing metadata does not include required Resource object" );
            return false;
        }

        Metadata = temp.ReturnValue!;
        var resource = Metadata.PrimaryResource!;

        if( resource.ImageHeight != resource.ImageWidth )
        {
            Logger?.Error( "Bing metadata indicates non-square image dimensions, which is not supported" );
            return false;
        }

        SetRetrieverInfo( new BingMapRetrieverInfo( resource.ImageUrl,
                                                    resource.ImageUrlSubdomains.ToList(),
                                                    MapType.GetDescription(),
                                                    Metadata.Copyright,
                                                    new Uri(Metadata.BrandLogoUri),
                                                    GlobalConstants.Wgs84MaxLatitude,
                                                    180,
                                                    resource.ZoomMin,
                                                    resource.ZoomMax,
                                                    resource.ImageHeight ) );

        return true;
    }
}