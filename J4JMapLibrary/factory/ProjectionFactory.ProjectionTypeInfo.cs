using System.Reflection;

namespace J4JMapLibrary;

public partial class ProjectionFactory
{
    private record ProjectionTypeInfo : TypeInfoBase
    {
        private static readonly ParameterType[] TiledParameters =
        {
            ParameterType.MapServer, ParameterType.Logger, ParameterType.TileCache
        };

        private static readonly ParameterType[] StaticParameters =
        {
            ParameterType.MapServer, ParameterType.Logger
        };

        private static readonly ParameterType[] TiledCredentialedParameters =
        {
            ParameterType.MapServer, ParameterType.Logger, ParameterType.TileCache, ParameterType.Credentials
        };

        private static readonly ParameterType[] StaticCredentialedParameters =
        {
            ParameterType.MapServer, ParameterType.Logger, ParameterType.Credentials
        };

        public ProjectionTypeInfo(
            Type projType
        )
            : base( projType )
        {
            ProjectionType = projType;

            var attr = projType.GetCustomAttribute<ProjectionAttribute>();
            Name = attr?.Name ?? string.Empty;
            ServerType = attr?.MapServerType;

            if( projType.IsAssignableTo( typeof( IStaticProjection ) ) )
            {
                BasicConstructor = GetConstructor(StaticParameters);
                ConfigurationCredentialConstructor = GetConstructor(StaticCredentialedParameters);
            }
            else
            {
                IsTiled = true;
                BasicConstructor = GetConstructor(TiledParameters);
                ConfigurationCredentialConstructor = GetConstructor(TiledCredentialedParameters);
            }
        }

        public string Name { get; }
        public Type ProjectionType { get; }
        public Type? ServerType { get; }
        public bool IsTiled { get; }

        public List<ParameterInfo>? BasicConstructor { get; }
        public List<ParameterInfo>? ConfigurationCredentialConstructor { get; }
    }
}
