using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using J4JSoftware.J4JMapLibrary;

namespace MapLibTests;

public class LatLongTests
{
    [Theory]
    [InlineData("37.5", true, 37.5)]
    [InlineData("37.5S", true, -37.5)]
    [InlineData("37.5south", true, -37.5)]
    [InlineData("37.5 S", true, -37.5)]
    [InlineData("37.5 SoUTH", true, -37.5)]
    [InlineData("37.5N", true, 37.5)]
    [InlineData("37.5north", true, 37.5)]
    [InlineData("37.5 n", true, 37.5)]
    [InlineData("37.5 NorTh", true, 37.5)]
    [InlineData("37.5SE", false, -37.5)]
    [InlineData("37.5 SE", false, -37.5)]
    [InlineData("37.5W", false, -37.5)]
    public void ParseLatitude( string text, bool succeed, float correct )
    {
        MapExtensions.TryParseToLatitude( text, out var parsed ).Should().Be( succeed );

        if( succeed)
            parsed.Should().Be( correct );
    }

    [Theory]
    [InlineData("37.5", true, 37.5)]
    [InlineData("37.5W", true, -37.5)]
    [InlineData("37.5west", true, -37.5)]
    [InlineData("37.5 W", true, -37.5)]
    [InlineData("37.5 west", true, -37.5)]
    [InlineData("37.5E", true, 37.5)]
    [InlineData("37.5east", true, 37.5)]
    [InlineData("37.5 e", true, 37.5)]
    [InlineData("37.5 EaSt", true, 37.5)]
    [InlineData("37.5SW", false, -37.5)]
    [InlineData("37.5 SW", false, -37.5)]
    [InlineData("37.5N", false, -37.5)]
    public void ParseLongitude(string text, bool succeed, float correct)
    {
        MapExtensions.TryParseToLongitude(text, out var parsed).Should().Be(succeed);

        if (succeed)
            parsed.Should().Be(correct);
    }

    [ Theory ]
    [InlineData("37.5,-122", true, 37.5F, -122)]
    [InlineData("37.5E,-122", false, 37.5F, -122)]
    [InlineData("37.5W,-122", false, -37.5F, -122)]
    [InlineData("-37.5W,-122", true, -37.5F, -122)]
    [InlineData("-100,-190", true, null, -180)]
    public void ParseLatLong( string text, bool succeed, float? correctLat, float correctLong )
    {
        MapExtensions.TryParseToLatLong(text, out var latitude, out var longitude).Should().Be(succeed);

        if( !succeed )
            return;

        if( correctLat == null )
        {
            var parts = text.Split( ',', StringSplitOptions.RemoveEmptyEntries );
            parts.Length.Should().Be( 2 );

            float.TryParse( parts[ 0 ], out var temp ).Should().BeTrue();

            correctLat = MapConstants.Wgs84MaxLatitude * Math.Sign( temp );
            latitude.Should().Be( correctLat );
        }
        else latitude.Should().Be(correctLat);

        longitude.Should().Be(correctLong);
    }
}
