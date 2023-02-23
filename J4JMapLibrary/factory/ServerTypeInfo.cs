namespace J4JSoftware.J4JMapLibrary;

internal class ServerTypeInfo : SupportingTypeInfo
{
    public ServerTypeInfo(
        Type serverType
    )
        : base( serverType, SupportingEntity.Server )
    {
    }
}
