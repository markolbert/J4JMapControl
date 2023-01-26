namespace J4JMapLibrary;

public class StaticConfiguration : SourceConfiguration, IStaticConfiguration
{
    public StaticConfiguration()
    {
        ConfigurationStyle = ServerConfiguration.Static;
    }

    public string RetrievalUrl { get; set; } = string.Empty;
    public int MinScale { get; set; }
    public int MaxScale { get; set; }
    public int TileHeightWidth { get; set; }
}