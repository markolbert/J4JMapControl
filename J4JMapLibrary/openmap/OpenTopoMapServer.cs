using J4JSoftware.Logging;

namespace J4JMapLibrary;

[ MapServer( "OpenTopoMaps", typeof( string ) ) ]
public class OpenTopoMapServer : OpenMapServer, IOpenMapServer
{
    public OpenTopoMapServer()
    {
        MinScale = 0;
        MaxScale = 15;
        MaxRequestLatency = 5000;
        RetrievalUrl = "https://tile.opentopomap.org/ZoomLevel/XTile/YTile.png";
        Copyright = "© OpenTopoMap-Mitwirkende, SRTM | Kartendarstellung\n© OpenTopoMap\nCC-BY-SA";
        CopyrightUri = new Uri( "http://opentopomap.org/" );
    }
}
