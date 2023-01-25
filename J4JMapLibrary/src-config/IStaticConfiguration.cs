namespace J4JMapLibrary;

public interface IStaticConfiguration : ISourceConfiguration
{
    string RetrievalUrl { get; }
    int MinScale { get; }
    int MaxScale { get; }
    int TileHeightWidth { get; }
}
