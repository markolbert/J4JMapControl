using FluentAssertions;
using J4JSoftware.J4JMapLibrary;

namespace MapLibTests;

public class FactoryTests : TestBase
{
    [ Theory ]
    [InlineData("BingMaps", true)]
    [InlineData("OpenStreetMaps", true)]
    [InlineData("OpenTopoMaps", true)]
    [InlineData("GoogleMaps", true)]
    [InlineData("UnknownMaps", false)]
    public async Task CreateProjectionFromName( string projectionName, bool testResult )
    {
        var projection = await CreateProjection(projectionName, null);

        if( testResult )
            projection.Should().NotBeNull();
        else projection.Should().BeNull();
    }

    [Theory]
    [InlineData("BingMaps",".jpeg")]
    [InlineData("OpenStreetMaps", ".png")]
    [InlineData("OpenTopoMaps", ".png")]
    [InlineData("GoogleMaps", ".png")]
    public async Task CheckImageFileExtension(string projectionName, string fileExtension)
    {
        var projection = await CreateProjection(projectionName);
        projection.Should().NotBeNull();
        projection!.MapServer.ImageFileExtension.Should().BeEquivalentTo( fileExtension );
    }

}
