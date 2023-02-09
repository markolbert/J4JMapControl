namespace J4JMapLibrary;

[ MapServer( "OpenStreetMaps", typeof( string ) ) ]
public class OpenStreetMapServer : OpenMapServer, IOpenMapServer
{
    public OpenStreetMapServer()
    {
        MinScale = 0;
        MaxScale = 20;
        RetrievalUrl = "https://tile.openstreetmap.org/ZoomLevel/XTile/YTile.png";
        Copyright = "© OpenStreetMap Contributors";
        CopyrightUri = new Uri( "http://www.openstreetmap.org/copyright" );
    }
}
