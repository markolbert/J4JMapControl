using J4JSoftware.J4JMapLibrary;

namespace J4JSoftware.J4JMapWinLibrary;

public interface ICredentialsDialog
{
    ICredentials? Credentials { get; }
    bool CancelOnFailure { get; }
}
