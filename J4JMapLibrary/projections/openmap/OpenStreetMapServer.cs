namespace J4JMapLibrary;

[ MapServer( "OpenStreetMaps", typeof( string ) ) ]
public sealed class OpenStreetMapServer : OpenMapServer, IOpenMapServer
{
    public OpenStreetMapServer()
    {
        MinScale = 0;
        MaxScale = 20;
        ScaleRange = new MinMax<int>( MinScale, MaxScale );
        RetrievalUrl = "https://tile.openstreetmap.org/{zoom}/{x}/{y}.png";
        Copyright = "© OpenStreetMap Contributors";
        CopyrightUri = new Uri( "http://www.openstreetmap.org/copyright" );
    }
}
