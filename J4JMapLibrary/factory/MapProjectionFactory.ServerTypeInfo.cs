using System.Reflection;

namespace J4JMapLibrary;

public partial class MapProjectionFactory
{
    private record ServerTypeInfo : TypeInfoBase
    {
        public ServerTypeInfo(
            Type serverType
        )
            : base( serverType )
        {
            ServerType = serverType;
            CredentialType = serverType.GetCustomAttribute<MapServerAttribute>()?.CredentialType;
        }

        public Type ServerType { get; }
        public Type? CredentialType { get; }
        public bool HasPublicParameterlessConstructor => Constructors.Any( x => x.Count == 0 );
    }
}
