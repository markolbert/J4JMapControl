namespace J4JMapLibrary;

public class DynamicConfiguration : SourceConfiguration, IDynamicConfiguration
{
    public DynamicConfiguration( string name )
        : base( name )
    {
    }

    public string MetadataRetrievalUrl { get; set; } = string.Empty;
}
