namespace J4JMapLibrary;

public interface IProjectionExtract
{
    bool Add(IMapFragment mapFragment);
    void Remove(IMapFragment mapFragment);
    void RemoveAt(int idx);
    void Clear();
    IAsyncEnumerable<IMapFragment> GetTilesAsync(CancellationToken ctx = default);
}
