using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Web.Http;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Media;

namespace J4JSoftware.J4JMapControl;

public class BingMapsImageRetriever : TileBasedImageRetriever<MultiTileCoordinates>
{
    public static MapRetrieverInfo GetRetrieverInfo( BingMapType mapType ) =>
        new( mapType.GetDescription(),
             "© Microsoft Corporation",
             new Uri( "https://www.microsoft.com/en-us/maps/product/enduserterms" ),
             GlobalConstants.Wgs84MaxLatitude,
             180,
             1,
             21,
             256 );

    public enum BingMapType
    {
        [ Description( "Bing (Aerial)" ) ]
        Aerial,

        [ Description( "Bing (Aerial w/Labels)" ) ]
        AerialWithLabels,

        [ Description( "Bing (Roads)" ) ]
        Roads
    }

    private readonly string _apiKey;

    public BingMapsImageRetriever(
        string apiKey,
        BingMapType mapType,
        IApplicationInfo appInfo,
        IJ4JLogger? logger
    )
        : base( GetRetrieverInfo( mapType ),
                "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/Mode?output=xml&key=ApiKey",
                appInfo,
                logger )
    {
        _apiKey = apiKey;
        MapType = mapType;
    }

    public BingMapType MapType { get; }

    protected override Uri GetRequestUri( MultiTileCoordinates tile ) =>
        new( RetrievalUriTemplate.Replace( "Mode", MapType.ToString() )
                                     .Replace( "ApiKey", _apiKey ) );

    protected override bool TryGetRequest( MultiTileCoordinates tile, out HttpRequestMessage? result )
    {
        result = new HttpRequestMessage(HttpMethod.Get, GetRequestUri(tile));

        return true;
    }
}
