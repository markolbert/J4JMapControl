namespace J4JMapLibrary;

public class ProjectionCredentials : IProjectionCredentials
{
    public List<Credential> Credentials { get; set; } = new();

    public bool TryGetCredential(string name, out string? result)
    {
        result = Credentials
           .FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Key;

        return result != null;
    }
}
