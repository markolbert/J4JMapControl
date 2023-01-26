namespace J4JMapLibrary;

public interface ILibraryConfiguration
{
    List<ISourceConfiguration> SourceConfigurations { get; set; }
    List<Credential> Credentials { get; set; }
    bool ValidateConfiguration();
}
