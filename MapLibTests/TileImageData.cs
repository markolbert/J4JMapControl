using System.Collections;

namespace MapLibTests;

public class TileImageData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { 1, 0, 0 };
        yield return new object[] { 2, 0, 0 };
        yield return new object[] { 5, 27, 27 };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
