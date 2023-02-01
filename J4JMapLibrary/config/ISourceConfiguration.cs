namespace J4JMapLibrary;

public interface ISourceConfiguration
{
    string Name { get; }
    int MaxRequestLatency { get; }
    string Copyright { get; }
    Uri? CopyrightUri { get; }
    float MaxLatitude { get; }
    float MinLatitude { get; }
    float MaxLongitude { get; }
    float MinLongitude { get; }
}