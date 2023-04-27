using System.Reflection;

namespace J4JSoftware.J4JMapLibrary;

public class CredentialsBase : ICredentials
{
    protected CredentialsBase(
        Type projectionType
    )
    {
        ProjectionType = projectionType;

        var attribute = projectionType.GetCustomAttribute<ProjectionAttribute>();
        ProjectionName = attribute?.ProjectionName ?? string.Empty;
    }

    public Type ProjectionType { get; }
    public string ProjectionName { get; }
}
