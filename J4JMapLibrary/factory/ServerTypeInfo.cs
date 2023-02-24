using System.Reflection;

namespace J4JSoftware.J4JMapLibrary;

internal record ServerTypeInfo
{
    public ServerTypeInfo(
        Type serverType
    )
    {
        var attr = serverType.GetCustomAttribute<MapServerAttribute>(false)
         ?? throw new NullReferenceException(
                $"{serverType} is not decorated with a {typeof(MapServerAttribute)}");

        Name = attr.ServerName;
        ProjectionType = attr.ProjectionType;
        ServerType = serverType;
    }

    public string Name { get; init; }
    public Type ServerType { get; init; }
    public Type ProjectionType { get; init; }
}
