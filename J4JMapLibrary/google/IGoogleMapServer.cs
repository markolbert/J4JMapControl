namespace J4JMapLibrary;

public interface IGoogleMapServer : IMapServer<VariableMapTile, GoogleCredentials>
{
    GoogleMapType MapType { get; }
    GoogleImageFormat ImageFormat { get; set; }
}
