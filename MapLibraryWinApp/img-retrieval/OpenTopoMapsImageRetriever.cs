using System;
using Windows.Web.Http;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public class OpenTopoMapsImageRetriever : TileBasedImageRetriever<MultiTileCoordinates>
{
    public static MapRetrieverInfo RetrieverInfo { get; } = new MapRetrieverInfo( "OpenTopoMap",
        "© OpenTopoMap-Mitwirkende, SRTM | Kartendarstellung\n© OpenTopoMap\nCC-BY-SA",
        new Uri( "http://opentopomap.org/" ),
        GlobalConstants.Wgs84MaxLatitude,
        180,
        1,
        20,
        256 );

    public OpenTopoMapsImageRetriever(
        IApplicationInfo appInfo,
        IJ4JLogger? logger
    )
        : base( RetrieverInfo,
                "https://tile.opentopomap.org/ZoomLevel/XTile/YTile.png",
                appInfo,
                logger )
    {
    }

    protected override Uri GetRequestUri( MultiTileCoordinates tile ) =>
        new( RetrievalUriTemplate.Replace( "ZoomLevel", tile.Zoom.Level.ToString() )
                                 .Replace( "XTile", tile.TileCoordinates.X.ToString() )
                                 .Replace( "YTile", tile.TileCoordinates.Y.ToString() ) );

    protected override bool TryGetRequest(MultiTileCoordinates tile, out HttpRequestMessage? result)
    {
        result = null;

        if (string.IsNullOrEmpty(AppInfo.UserAgent))
        {
            Logger?.Error("Undefined or empty User-Agent");
            return false;
        }

        result = new HttpRequestMessage( HttpMethod.Get, GetRequestUri( tile ) );
        result.Headers.Add("User-Agent", AppInfo.UserAgent);

        return true;
    }
}
