namespace J4JMapLibrary;

public interface IGoogleMapServer : IMapServer<VariableMapTile, GoogleCredentials>
{
    GoogleMapType MapType { get; set; }
    GoogleImageFormat ImageFormat { get; set; }
}
