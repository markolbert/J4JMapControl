namespace J4JSoftware.J4JMapLibrary;

public class CredentialsSucceededEventArgs
{
    public CredentialsSucceededEventArgs(
        string projectionName,
        object credentials
    )
    {
        ProjectionName = projectionName;
        Credentials = credentials;
    }

    public string ProjectionName { get; }
    public object Credentials { get; }
}
