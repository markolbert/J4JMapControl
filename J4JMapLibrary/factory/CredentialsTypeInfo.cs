using System.Reflection;

namespace J4JSoftware.J4JMapLibrary;

internal record CredentialsTypeInfo
{
    public CredentialsTypeInfo(
        Type credentialsType
    )
    {
        var attr = credentialsType.GetCustomAttribute<MapCredentialsAttribute>(false)
         ?? throw new NullReferenceException(
                $"{credentialsType} is not decorated with a {typeof(MapCredentialsAttribute)}");

        Name = attr.CredentialsName;
        ProjectionType = attr.ProjectionType;
        CredentialsType = credentialsType;
    }

    public string Name { get; init; }
    public Type CredentialsType { get; init; }
    public Type ProjectionType { get; init; }
}
