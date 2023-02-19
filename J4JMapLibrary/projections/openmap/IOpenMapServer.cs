namespace J4JSoftware.J4JMapLibrary;

public interface IOpenMapServer : IMapServer
{
    string RetrievalUrl { get; }
    string UserAgent { get; }
    Task<bool> InitializeAsync( string credentials, CancellationToken ctx = default );
}
