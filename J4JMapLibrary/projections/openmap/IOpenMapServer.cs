namespace J4JSoftware.J4JMapLibrary;

public interface IOpenMapServer : IMapServer
{
    string RetrievalUrl { get; }
    string UserAgent { get; }
    Task<bool> InitializeAsync( IOpenMapCredentials credentials, CancellationToken ctx = default );
}

// marker interface
public interface IOpenStreetMapsServer : IOpenMapServer
{
}


// marker interface
public interface IOpenTopoMapsServer : IOpenMapServer
{
}
