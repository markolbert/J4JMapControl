namespace J4JMapLibrary;

[ AttributeUsage( AttributeTargets.Class, Inherited = false ) ]
public class MapProjectionAttribute : Attribute
{
    public MapProjectionAttribute(
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
