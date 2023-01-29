namespace J4JMapLibrary;

public class Credential
{
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;

    public bool IsValid => !string.IsNullOrEmpty( Name ) && !string.IsNullOrEmpty( Key );
}
