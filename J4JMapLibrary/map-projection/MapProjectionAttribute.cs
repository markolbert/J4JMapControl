namespace J4JMapLibrary;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited=false)]
public class MapProjectionAttribute : Attribute
{
    public MapProjectionAttribute(
        string name
    )
    {
        Name = name;
    }

    public string Name { get; }
}