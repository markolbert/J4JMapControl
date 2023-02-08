using System.Reflection;

namespace J4JMapLibrary;

public partial class MapProjectionFactory
{
    private record SourceConfigurationInfo(
        string Name,
        bool HasCredentialsConstructor,
        Type MapProjectionType,
        Type ServerType,
        Type CredentialsType
    );

    private record ProjectionTypeInfo
    {
        public ProjectionTypeInfo(
            Type projType
        )
        {
            ProjectionType = projType;

            var attr = projType.GetCustomAttribute<MapProjectionAttribute>();
            Name = attr?.Name ?? string.Empty;
            ServerType = attr?.MapServerType;
        }

        public string Name { get; }
        public Type ProjectionType { get; }
        public Type? ServerType { get; }
    }

    private record ServerTypeInfo( Type ServerType )
    {
        public Type? CredentialType { get; } = ServerType.GetCustomAttribute<MapServerAttribute>()?.CredentialType;
    }
}
