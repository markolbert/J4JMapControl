namespace J4JMapLibrary;

[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
public class MapProjectionAttribute : Attribute
{
    public MapProjectionAttribute(
        string name,
        ServerConfigurationStyle serverConfig
    )
    {
        Name = name;
        ServerConfigurationStyle = serverConfig;
    }

    public string Name { get; }
    public ServerConfigurationStyle ServerConfigurationStyle { get; }
}