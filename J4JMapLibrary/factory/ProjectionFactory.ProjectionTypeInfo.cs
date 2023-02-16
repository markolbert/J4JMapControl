using System.Reflection;

namespace J4JMapLibrary;

public partial class ProjectionFactory
{
    private record ProjectionTypeInfo : TypeInfoBase
    {
        private static readonly ParameterType[] TiledParameters =
        {
            ParameterType.Logger, ParameterType.TileCache
        };

        private static readonly ParameterType[] TiledCredentialedParameters =
        {
            ParameterType.Logger, ParameterType.TileCache, ParameterType.Credentials
        };

        private static readonly ParameterType[] StaticParameters =
        {
            ParameterType.Logger
        };

        private static readonly ParameterType[] StaticCredentialedParameters =
        {
            ParameterType.Logger, ParameterType.Credentials
        };

        public ProjectionTypeInfo(
            Type projType
        )
            : base( projType )
        {
            ProjectionType = projType;

            var attr = projType.GetCustomAttribute<ProjectionAttribute>();
            Name = attr?.Name ?? string.Empty;

            if( projType.IsAssignableTo( typeof( ITiledProjection ) ) )
            {
                IsTiled = true;
                BasicConstructor = GetConstructor(TiledParameters);
                ConfigurationCredentialConstructor = GetConstructor(TiledCredentialedParameters);
            }
            else
            {
                BasicConstructor = GetConstructor(StaticParameters);
                ConfigurationCredentialConstructor = GetConstructor(StaticCredentialedParameters);
            }
        }

        public string Name { get; }
        public Type ProjectionType { get; }
        public bool IsTiled { get; }

        public List<ParameterInfo>? BasicConstructor { get; }
        public List<ParameterInfo>? ConfigurationCredentialConstructor { get; }
    }
}
