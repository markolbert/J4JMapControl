namespace J4JMapLibrary;

[ MapServer( "OpenStreetMaps" ) ]
public sealed class OpenStreetMapServer : OpenMapServer
{
    public OpenStreetMapServer()
    {
        MinScale = 0;
        MaxScale = 20;
        RetrievalUrl = "https://tile.openstreetmap.org/{zoom}/{x}/{y}.png";
        Copyright = "© OpenStreetMap Contributors";
        CopyrightUri = new Uri( "http://www.openstreetmap.org/copyright" );
    }
}
