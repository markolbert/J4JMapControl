namespace J4JMapLibrary;

[ AttributeUsage( AttributeTargets.Class, Inherited = false ) ]
public class ProjectionAttribute : Attribute
{
    public ProjectionAttribute(
        string name,
        Type mapServerType
    )
    {
        Name = name;
        MapServerType = mapServerType;
    }

    public string Name { get; }
    public Type MapServerType { get; }
}
