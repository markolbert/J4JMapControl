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
    public async Task CreateProjectionFromName( string projectionName, bool testResult )
    {
        var factory = GetFactory();
        factory.Should().NotBeNull();

        var result= await factory.CreateMapProjection( projectionName, null );

        if( testResult )
            result.Projection.Should().NotBeNull();
        else result.Projection.Should().BeNull();
    }

    [Theory]
    [InlineData(typeof(BingMapsProjection), true)]
    [InlineData(typeof(OpenStreetMapsProjection), true)]
    [InlineData(typeof(OpenTopoMapsProjection), true)]
    [InlineData(typeof(GoogleMapsProjection), true)]
    public async Task CreateProjectionFromType(Type projectionType, bool testResult)
    {
        var factory = GetFactory();
        factory.Should().NotBeNull();

        var result = await factory.CreateMapProjection( projectionType, null );

        if (testResult)
            result.Projection.Should().NotBeNull();
        else result.Projection.Should().BeNull();
    }

    [Theory]
    [InlineData(typeof(BingMapsProjection),".jpeg")]
    [InlineData(typeof(OpenStreetMapsProjection), ".png")]
    [InlineData(typeof(OpenTopoMapsProjection), ".png")]
    [InlineData(typeof(GoogleMapsProjection), ".png")]
    public async Task CheckImageFileExtension(Type type, string fileExtension)
    {
        var result = await GetFactory().CreateMapProjection(type, null);
        result.Projection.Should().NotBeNull();

        result.Projection!.MapServer.ImageFileExtension.Should().BeEquivalentTo( fileExtension );
    }

}
