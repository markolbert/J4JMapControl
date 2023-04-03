namespace J4JSoftware.J4JMapLibrary;

[ AttributeUsage( AttributeTargets.Class, Inherited = false ) ]
public class MapCredentialsAttribute : Attribute
{
    public MapCredentialsAttribute(
        string credentialsName,
        Type projectionType
    )
    {
        CredentialsName = credentialsName;
        ProjectionType = projectionType;
    }

    public string CredentialsName { get; }
    public Type ProjectionType { get; }
}
