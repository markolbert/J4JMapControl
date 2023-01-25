namespace J4JMapLibrary;

public interface ILibraryConfiguration
{
    List<SourceConfiguration> SourceConfigurations { get; set; }
    List<Credential> Credentials { get; set; }
    bool ValidateConfiguration();
}
