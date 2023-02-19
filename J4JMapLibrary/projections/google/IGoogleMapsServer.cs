namespace J4JSoftware.J4JMapLibrary;

public interface IGoogleMapsServer : IMapServer
{
    string ApiKey { get; }
    string Signature { get; }
    GoogleMapType MapType { get; }
    Task<bool> InitializeAsync(GoogleCredentials credentials, CancellationToken ctx = default);
}
