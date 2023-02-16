using System.Collections;
using J4JMapLibrary;

namespace MapLibTests;

public class StaticImageData : IEnumerable<object[]>
{
    public record Region( float Latitude, float Longitude, int Scale, float Height, float Width )
    {
        public string FragmentId =>
            $"{MapExtensions.LatitudeToText( Latitude )}-{MapExtensions.LongitudeToText( Longitude )}-{Scale}-{Height}-{Width}";
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { new Region(0,0,0, 512, 512) };
        yield return new object[] { new Region(0,0,2, 512, 512) };
        yield return new object[] { new Region( 37, -122, 5, 512, 512) };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}