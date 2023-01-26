namespace J4JMapLibrary;

public interface ILibraryConfiguration
{
    List<SourceConfiguration> SourceConfigurations { get; set; }
    List<Credential> Credentials { get; set; }
    bool ValidateConfiguration();

    bool TryGetConfiguration( string name, out SourceConfiguration? result );
    bool TryGetCredential( string name, out string? result );
}
