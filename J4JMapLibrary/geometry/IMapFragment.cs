namespace J4JMapLibrary;

public interface IMapFragment
{
    IMapServer MapServer { get; }
    int MaxRequestLatency { get; }
    long ImageBytes { get; }
    event EventHandler? ImageChanged;
    Task<byte[]?> GetImageAsync( int scale, bool forceRetrieval = false, CancellationToken ctx = default );
}
