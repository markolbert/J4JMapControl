using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using Microsoft.UI.Xaml.Media;

namespace J4JSoftware.J4JMapControl;

public class BingMapsImageRetriever : TileBasedImageRetriever<MultiTileCoordinates>
{
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
        IJ4JLogger? logger
    )
        : base( "http://dev.virtualearth.net/REST/V1/Imagery/Metadata/{Mode}?output=xml&key={ApiKey}",
                mapType.GetDescription(),
                "© Microsoft Corporation",
                new Uri( "https://www.microsoft.com/en-us/maps/product/enduserterms" ),
                logger )
    {
        _apiKey = apiKey;
        MapType = mapType;
    }

    public BingMapType MapType { get; }

    protected override bool TryGetRequestUri( MultiTileCoordinates tile, out Uri? result )
    {
        throw new NotImplementedException();
    }

    protected override bool TryGetClientHandler( out HttpClientHandler? result )
    {
        return base.TryGetClientHandler( out result );
    }
}
