namespace J4JMapLibrary;

public class StaticConfiguration : SourceConfiguration, IStaticConfiguration
{
    public StaticConfiguration(
        string name
    )
        : base( name )
    {
    }

    public string RetrievalUrl { get; set; } = string.Empty;
    public int MinScale { get; set; }
    public int MaxScale { get; set; }
    public int TileHeightWidth { get; set; }
}