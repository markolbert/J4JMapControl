using System.Collections;

namespace MapLibTests;

public class StaticImageData : IEnumerable<object[]>
{
    public record Region( float Latitude, float Longitude, int Scale, float Height, float Width, string FileId );

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { new Region(0,0,0, 512, 512, "000HW512") };
        yield return new object[] { new Region(0,0,2, 512, 512, "002HW512") };
        yield return new object[] { new Region( 37, -122, 5, 512, 512,"37N122W5HW512") };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}