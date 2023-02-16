namespace J4JMapLibrary;

public interface IMapFragment
{
    public string FragmentId { get; }

    IMapServer MapServer { get; }
    int MaxRequestLatency { get; }
    public byte[]? ImageData { get; }
    long ImageBytes { get; }
    event EventHandler? ImageChanged;
    Task<byte[]?> GetImageAsync( int scale, bool forceRetrieval = false, CancellationToken ctx = default );
}
