namespace J4JMapLibrary;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited=false)]
public class MapServerAttribute : Attribute
{
    public MapServerAttribute(
        string projectionName,
        Type credentialType
    )
    {
        ProjectionName = projectionName;
        CredentialType = credentialType;
    }

    public string ProjectionName { get; }
    public Type CredentialType { get; }
}