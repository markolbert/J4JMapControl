using System.Collections;

namespace MapLibTests;

public class TileImageData : IEnumerable<object[]>
{
    public record Tile(int Scale, int TileX, int TileY);

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { new Tile(1, 0, 0) };
        yield return new object[] { new Tile(2, 0, 0) };
        yield return new object[] { new Tile(5, 27, 27) };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}