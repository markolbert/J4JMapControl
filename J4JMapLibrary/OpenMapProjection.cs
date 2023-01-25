using J4JSoftware.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace J4JMapLibrary;

public class OpenMapProjection : TiledProjection
{
    private readonly string _retrievalUrl;

    private string _userAgent = string.Empty;

    public OpenMapProjection(
        IStaticConfiguration staticConfig,
        IJ4JLogger logger
    )
        : base( staticConfig, logger )
    {
        _retrievalUrl = staticConfig.RetrievalUrl;

        TileHeightWidth = staticConfig.TileHeightWidth;
        MinScale = staticConfig.MinScale;
        MaxScale = staticConfig.MaxScale;

        SetSizes( 0 );
    }

    public void Initialize( string userAgent )
    {
        _userAgent = userAgent;
        Initialized = true;

        Scale = MinScale;
    }

    protected override bool TryGetRequest( MapTile coordinates, out HttpRequestMessage? result )
    {
        result = null;

        if( !Initialized )
            return false;

        coordinates = Cap( coordinates )!;

        if( string.IsNullOrEmpty( _userAgent ) )
        {
            Logger?.Error( "Undefined or empty User-Agent" );
            return false;
        }

        var uriText = _retrievalUrl.Replace( "ZoomLevel", Scale.ToString() )
                                   .Replace( "XTile", coordinates.X.ToString() )
                                   .Replace( "YTile", coordinates.Y.ToString() );

        result = new HttpRequestMessage( HttpMethod.Get, new Uri( uriText ) );
        result.Headers.Add( "User-Agent", _userAgent );

        return true;
    }
}
