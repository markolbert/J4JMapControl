using System;
using System.Threading.Tasks;
using Windows.Web.Http;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public class OpenTopoMapsImageRetriever : ImageDirectImageRetriever<MultiTileCoordinates>
{
    private readonly string _userAgent;

    public OpenTopoMapsImageRetriever(
        IApplicationInfo appInfo,
        IJ4JLogger? logger
    )
        : base( logger )
    {
        _userAgent = appInfo.UserAgent;

        SetRetrieverInfo( new MapRetrieverInfo( "https://tile.opentopomap.org/ZoomLevel/XTile/YTile.png",
                                                "OpenTopoMap",
                                                "© OpenTopoMap-Mitwirkende, SRTM | Kartendarstellung\n© OpenTopoMap\nCC-BY-SA",
                                                new Uri( "http://opentopomap.org/" ),
                                                GlobalConstants.Wgs84MaxLatitude,
                                                180,
                                                1,
                                                20,
                                                256 ) );
    }

    protected override HttpRequestMessage? GetRequest( MultiTileCoordinates tile )
    {
        if (string.IsNullOrEmpty(_userAgent))
        {
            Logger?.Error("Undefined or empty User-Agent");
            return null;
        }

        if( MapRetrieverInfo == null )
        {
            Logger?.Error("Undefined MapRetrieverInfo");
            return null;
        }

        var uriText = MapRetrieverInfo.RetrievalUrl.Replace( "ZoomLevel", tile.Zoom.Level.ToString() )
                                      .Replace( "XTile", tile.TileCoordinates.X.ToString() )
                                      .Replace( "YTile", tile.TileCoordinates.Y.ToString() );

        var retVal = new HttpRequestMessage(HttpMethod.Get, new Uri(uriText));
        retVal.Headers.Add("User-Agent", _userAgent);

        return retVal;
    }
}
