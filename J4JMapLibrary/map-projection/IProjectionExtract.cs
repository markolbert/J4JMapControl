namespace J4JMapLibrary;

public interface IProjectionExtract
{
    bool Add(IMapTile tile);
    void Remove(IMapTile tile);
    void RemoveAt(int idx);
    void Clear();
    IAsyncEnumerable<IMapTile> GetTilesAsync(CancellationToken ctx = default);
}
