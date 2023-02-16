namespace J4JMapLibrary;

[ AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = false ) ]
public class MapServerAttribute : Attribute
{
    public MapServerAttribute(
        string projectionName
    )
    {
        ProjectionName = projectionName;
    }

    public string ProjectionName { get; }
}
