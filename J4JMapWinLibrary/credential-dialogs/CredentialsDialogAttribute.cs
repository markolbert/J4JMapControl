using System;
using J4JSoftware.J4JMapLibrary;

namespace J4JSoftware.J4JMapWinLibrary;

public class CredentialsDialogAttribute : Attribute
{
    public CredentialsDialogAttribute(
        Type credentialsType
    )
    {
        if( !credentialsType.IsAssignableTo( typeof( ICredentials ) ) )
            throw new ArgumentException( $"{credentialsType} is not assignable to {typeof( ICredentials )}" );

        CredentialsType = credentialsType;
    }

    public Type CredentialsType { get; }
}
