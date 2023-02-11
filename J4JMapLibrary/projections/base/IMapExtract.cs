namespace J4JMapLibrary;

public interface IMapExtract
{
    bool Add(IMapFragment mapFragment);
    void Remove(IMapFragment mapFragment);
    void RemoveAt(int idx);
    void Clear();
    IAsyncEnumerable<IMapFragment> GetTilesAsync(int scale, CancellationToken ctx = default);
}
