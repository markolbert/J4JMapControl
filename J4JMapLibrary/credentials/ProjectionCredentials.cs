using Microsoft.Extensions.Configuration;

namespace J4JMapLibrary;

public class ProjectionCredentials : IProjectionCredentials
{
    public ProjectionCredentials(
        IConfiguration config
    )
    {
        // we can't use the built-in IConfiguration stuff because
        // the objects we're creating are polymorphic (either a Credential
        // or a SignedCredential)
        var idx = 0;

        // thanx to Métoule for this!
        // https://stackoverflow.com/questions/55954858/how-to-load-polymorphic-objects-in-appsettings-json
        while ( true )
        {
            var name = config.GetValue<string>( $"Credentials:{idx}:Name" );
            var apiKey = config.GetValue<string>( $"Credentials:{idx}:ApiKey" );
            var signature = config.GetValue<string>( $"Credentials:{idx}:SignatureSecret" );

            if( string.IsNullOrEmpty( name ) || string.IsNullOrEmpty( apiKey ) )
                break;

            var credential = string.IsNullOrEmpty( signature )
                ? new Credential { ApiKey = apiKey, Name = name }
                : new SignedCredential { ApiKey = apiKey, Name = name, Signature = signature };

            Credentials.Add( credential );

            idx++;
        }
    }

    public List<Credential> Credentials { get; } = new();

    public bool TryGetCredential( string projectionName, out Credential? result )
    {
        result = Credentials
                .FirstOrDefault( x => x.Name.Equals( projectionName, StringComparison.OrdinalIgnoreCase ) );

        return result != null;
    }
}
