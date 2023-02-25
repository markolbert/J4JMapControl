using System.Collections;
using J4JSoftware.J4JMapLibrary;

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
        yield return new object[] { new Region(0,0,0, 256, 256) };
        yield return new object[] { new Region(0,0,12, 256, 256) };
        yield return new object[] { new Region( 37, -122, 12, 256, 256) };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}