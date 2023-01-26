namespace J4JMapLibrary;

public class DynamicConfiguration : SourceConfiguration, IDynamicConfiguration
{
    public DynamicConfiguration()
        : base( ServerConfiguration.Dynamic )
    {
    }

    public string MetadataRetrievalUrl { get; set; } = string.Empty;
}
