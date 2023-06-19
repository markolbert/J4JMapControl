namespace J4JSoftware.J4JMapLibrary;

public abstract partial class Projection
{
    public string Name { get; } = string.Empty;
    public bool Initialized { get; protected set; }
    public int MaxRequestLatency { get; set; } = DefaultMaxRequestLatency;
    public string ImageFileExtension { get; protected set; } = string.Empty;
    public string Copyright { get; protected set; } = string.Empty;
    public Uri? CopyrightUri { get; protected set; }
}
