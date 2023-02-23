namespace J4JSoftware.J4JMapLibrary;

internal class CredentialsTypeInfo : SupportingTypeInfo
{
    public CredentialsTypeInfo(
        string name,
        Type supportingType,
        Type projType
    )
        : base( supportingType, SupportingEntity.Credentials )
    {
    }
}
