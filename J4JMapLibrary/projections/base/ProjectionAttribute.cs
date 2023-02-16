namespace J4JMapLibrary;

[ AttributeUsage( AttributeTargets.Class, Inherited = false ) ]
public class ProjectionAttribute : Attribute
{
    public ProjectionAttribute(
        string name
    )
    {
        Name = name;
    }

    public string Name { get; }
}
