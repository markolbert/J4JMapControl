using J4JSoftware.J4JMapLibrary;

namespace J4JSoftware.J4JMapWinLibrary;

public record ValidCredentialsEventArgs( string ProjectionName, ICredentials Credentials, int AttemptNumber );
