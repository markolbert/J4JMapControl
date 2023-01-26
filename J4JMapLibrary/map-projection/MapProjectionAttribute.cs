namespace J4JMapLibrary;

[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
public class MapProjectionAttribute : Attribute
{
    public MapProjectionAttribute(
        string name,
        ServerConfiguration serverConfig
    )
    {
        Name = name;
        ServerConfiguration = serverConfig;
    }

    public string Name { get; }
    public ServerConfiguration ServerConfiguration { get; }
}