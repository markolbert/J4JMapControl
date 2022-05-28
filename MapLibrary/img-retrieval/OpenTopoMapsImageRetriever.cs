using System;
using Windows.Web.Http;
using J4JSoftware.Logging;

namespace J4JSoftware.MapLibrary;

public class OpenTopoMapsImageRetriever : TileBasedImageRetriever
{
    private readonly string _userAgent;

    public OpenTopoMapsImageRetriever(
        IApplicationInfo appInfo,
        IJ4JLogger? logger
    )
        : base( logger )
    {
        _userAgent = appInfo.UserAgent;
    }

    protected override MapRetrieverInfo? GetMapRetrieverInfo( IMapProjection mapProjection ) =>
        new("https://tile.opentopomap.org/ZoomLevel/XTile/YTile.png",
            "OpenTopoMap",
            "© OpenTopoMap-Mitwirkende, SRTM | Kartendarstellung\n© OpenTopoMap\nCC-BY-SA",
            new Uri( "http://opentopomap.org/" ),
            GlobalConstants.Wgs84MaxLatitude,
            180,
            0,
            20,
            256);

    protected override HttpRequestMessage? GetRequest( MapTile mapTile )
    {
        if (string.IsNullOrEmpty(_userAgent))
        {
            Logger?.Error("Undefined or empty User-Agent");
            return null;
        }

        var uriText = MapRetrieverInfo!.RetrievalUrl.Replace( "ZoomLevel", MapProjection.ZoomLevel.ToString() )
                                      .Replace( "XTile", mapTile.X.ToString() )
                                      .Replace( "YTile", mapTile.Y.ToString() );

        var retVal = new HttpRequestMessage(HttpMethod.Get, new Uri(uriText));
        retVal.Headers.Add("User-Agent", _userAgent);

        return retVal;
    }
}
