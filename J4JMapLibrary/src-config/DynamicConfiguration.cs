namespace J4JMapLibrary;

public class DynamicConfiguration : SourceConfiguration, IDynamicConfiguration
{
    public string MetadataRetrievalUrl { get; set; } = string.Empty;
}
