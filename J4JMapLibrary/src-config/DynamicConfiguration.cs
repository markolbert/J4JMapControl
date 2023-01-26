namespace J4JMapLibrary;

public class DynamicConfiguration : SourceConfiguration, IDynamicConfiguration
{
    public DynamicConfiguration()
    {
        ConfigurationStyle = ServerConfiguration.Dynamic;
    }

    public string MetadataRetrievalUrl { get; set; } = string.Empty;
}
