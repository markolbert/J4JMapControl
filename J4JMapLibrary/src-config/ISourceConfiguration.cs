namespace J4JMapLibrary;

public interface ISourceConfiguration
{
    string Name { get; }
    int MaxRequestLatency { get; }
    string Copyright { get; }
    Uri? CopyrightUri { get; }
    double MaxLatitude { get; }
    double MinLatitude { get; }
    double MaxLongitude { get; }
    double MinLongitude { get; }
}