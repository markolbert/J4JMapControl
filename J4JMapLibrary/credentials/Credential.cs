namespace J4JMapLibrary;

public class Credential
{
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;

    public virtual bool IsValid => !string.IsNullOrEmpty( Name ) && !string.IsNullOrEmpty( ApiKey );
}