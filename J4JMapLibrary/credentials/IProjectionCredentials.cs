using Microsoft.Extensions.Configuration;

namespace J4JMapLibrary;

public interface IProjectionCredentials
{
    List<Credential> Credentials { get; set; }

    bool TryGetCredential( string projectionName, out string? result );
}
