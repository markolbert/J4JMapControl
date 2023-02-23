namespace J4JSoftware.J4JMapLibrary;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class MapServerAttribute : Attribute
{
    public MapServerAttribute(
        string serverName,
        Type projectionType
    )
    {
        ServerName = serverName;
        ProjectionType = projectionType;
    }

    public string ServerName { get; }
    public Type ProjectionType { get; }
}