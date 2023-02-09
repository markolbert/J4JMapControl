namespace J4JMapLibrary;

public class ProjectionCredentials : IProjectionCredentials
{
    public List<Credential> Credentials { get; set; } = new();

    public bool TryGetCredential( string projectionName, out string? result )
    {
        result = Credentials
                .FirstOrDefault( x => x.Name.Equals( projectionName, StringComparison.OrdinalIgnoreCase ) )
               ?.Key;

        return result != null;
    }
}
