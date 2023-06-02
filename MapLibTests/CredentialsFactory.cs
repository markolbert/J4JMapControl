using J4JSoftware.J4JMapLibrary;

namespace MapLibTests;

internal class CredentialsFactory : CredentialsFactoryBase
{
    private readonly MapCredentials _credentials;

    public CredentialsFactory(
        MapCredentials credentials
    )
    {
        _credentials = credentials;
    }

    protected override ICredentials? CreateCredentials( Type credType )
    {
        if( credType == typeof( BingCredentials ) )
            return _credentials.BingCredentials;

        if( credType == typeof( GoogleCredentials ) )
            return _credentials.GoogleCredentials;

        if( credType == typeof( OpenStreetCredentials ) )
            return _credentials.OpenStreetCredentials;

        return credType == typeof( OpenTopoCredentials ) ? _credentials.OpenTopoCredentials : null;
    }
}
