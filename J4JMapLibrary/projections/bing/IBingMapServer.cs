namespace J4JSoftware.J4JMapLibrary;

public interface IBingMapServer : IMapServer
{
    string ApiKey { get; }
    string? CultureCode { get; }
    BingMapType MapType { get; }
    BingImageryMetadata? Metadata { get; }
    Task<bool> InitializeAsync(BingCredentials credentials, CancellationToken ctx = default);
}
