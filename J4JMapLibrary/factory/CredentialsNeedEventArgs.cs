namespace J4JSoftware.J4JMapLibrary;

public class CredentialsNeedEventArgs
{
    public CredentialsNeedEventArgs(
        string projectionName
    )
    {
        ProjectionName = projectionName;
    }

    public string ProjectionName { get; }
    public object? Credentials { get; set; }
    public bool CancelImmediately { get; set; }
    public bool CancelOnFailure { get; set; } = true;
}
