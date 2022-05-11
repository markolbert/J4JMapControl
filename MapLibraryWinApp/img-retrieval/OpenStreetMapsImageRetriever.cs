using System;
using Windows.Web.Http;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;

namespace J4JSoftware.J4JMapControl;

public class OpenStreetMapsImageRetriever : TileBasedImageRetriever
{
    private readonly string _userAgent;

    public OpenStreetMapsImageRetriever(
        IApplicationInfo appInfo,
        IJ4JLogger? logger
    )
        : base( logger )
    {
        _userAgent = appInfo.UserAgent;
    }

    protected override MapRetrieverInfo GetMapRetrieverInfo( IMapProjection mapProjection ) =>
        new( "https://tile.openstreetmap.org/ZoomLevel/XTile/YTile.png",
             "OpenStreetMap",
             "© OpenStreetMap Contributors",
             new Uri( "http://www.openstreetmap.org/copyright" ),
             GlobalConstants.Wgs84MaxLatitude,
             180,
             0,
             20,
             256 );

    protected override HttpRequestMessage? GetRequest( MultiCoordinates coordinates )
    {
        if( string.IsNullOrEmpty( _userAgent ) )
        {
            Logger?.Error( "Undefined or empty User-Agent" );
            return null;
        }

        var uriText = MapRetrieverInfo.RetrievalUrl.Replace( "ZoomLevel", MapProjection.ZoomLevel.ToString() )
                                      .Replace( "XTile", coordinates.TilePoint.X.ToString() )
                                      .Replace( "YTile", coordinates.TilePoint.Y.ToString() );

        var retVal = new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );
        retVal.Headers.Add( "User-Agent", _userAgent );

        return retVal;
    }
}
