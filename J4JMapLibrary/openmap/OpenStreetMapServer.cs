namespace J4JMapLibrary;

[ MapServer( "OpenStreetMaps", typeof( string ) ) ]
public class OpenStreetMapServer : OpenMapServer, IOpenMapServer
{
    public OpenStreetMapServer()
    {
        MinScale = 0;
        MaxScale = 20;
        RetrievalUrl = "https://mapFragment.openstreetmap.org/{zoom}/{x}/{y}.png";
        Copyright = "© OpenStreetMap Contributors";
        CopyrightUri = new Uri( "http://www.openstreetmap.org/copyright" );
    }
}
