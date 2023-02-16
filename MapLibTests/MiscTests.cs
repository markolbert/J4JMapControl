using J4JSoftware.J4JMapLibrary;
using FluentAssertions;

namespace MapLibTests;

public class MiscTests
{

    [ Theory ]
    [ InlineData( "0", 1, 0, 0 ) ]
    [ InlineData( "01", 2, 1, 0 ) ]
    [ InlineData( "032", 3, 2, 3 ) ]
    public void DecodeQuadKeys( string quadKey, int scale, int x, int y )
    {
        MapExtensions.TryParseQuadKey( quadKey, out var deconstructed )
                     .Should()
                     .BeTrue();

        deconstructed.Should().NotBeNull();

        deconstructed!.Scale.Should().Be( scale );
        deconstructed.XTile.Should().Be( x );
        deconstructed.YTile.Should().Be( y );
    }
}
