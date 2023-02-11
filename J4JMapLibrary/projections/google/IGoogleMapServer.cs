namespace J4JMapLibrary;

public interface IGoogleMapServer : IMapServer<StaticFragment, GoogleCredentials>
{
    GoogleMapType MapType { get; set; }
    GoogleImageFormat ImageFormat { get; set; }
}
