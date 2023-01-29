namespace J4JMapLibrary;

public interface IDynamicConfiguration : ISourceConfiguration
{
    string MetadataRetrievalUrl { get; }
}
