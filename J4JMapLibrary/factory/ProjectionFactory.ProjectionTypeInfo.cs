using System.Reflection;

namespace J4JMapLibrary;

public partial class ProjectionFactory
{
    private record ProjectionTypeInfo : TypeInfoBase
    {
        public static readonly ParameterType[] BaseParameterTypes =
        {
            ParameterType.MapServer, ParameterType.Logger, ParameterType.TileCache
        };

        public static readonly ParameterType[] CredentialedParameterTypes =
        {
            ParameterType.MapServer, ParameterType.Logger, ParameterType.TileCache, ParameterType.Credentials
        };

        public ProjectionTypeInfo(
            Type projType
        )
            : base( projType )
        {
            ProjectionType = projType;

            var attr = projType.GetCustomAttribute<MapProjectionAttribute>();
            Name = attr?.Name ?? string.Empty;
            ServerType = attr?.MapServerType;

            BasicConstructor = GetConstructor( BaseParameterTypes );
            ConfigurationCredentialConstructor = GetConstructor( CredentialedParameterTypes );
        }

        public string Name { get; }
        public Type ProjectionType { get; }
        public Type? ServerType { get; }

        public List<ParameterInfo>? BasicConstructor { get; }
        public List<ParameterInfo>? ConfigurationCredentialConstructor { get; }
    }
}
