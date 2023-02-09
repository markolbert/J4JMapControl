namespace J4JMapLibrary;

public interface IProjectionCredentials
{
    List<Credential> Credentials { get; }

    bool TryGetCredential( string projectionName, out Credential? result );
}
