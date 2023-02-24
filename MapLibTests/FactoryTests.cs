using FluentAssertions;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.DependencyInjection;

namespace MapLibTests;

public class FactoryTests : TestBase
{
    [ Theory ]
    [InlineData("BingMaps", true, true)]
    [InlineData("OpenStreetMaps", true, true)]
    [InlineData("OpenTopoMaps", true, true)]
    [InlineData("GoogleMaps", true, true)]
    [InlineData("UnknownMaps", false, false)]
    public void CreateProjectionFromName( string projectionName, bool projFound, bool authenticated )
    {
        var factory = J4JDeusEx.ServiceProvider.GetService<ProjectionFactory>();
        factory.Should().NotBeNull();

        var result = factory!.CreateProjection( projectionName );
        result.ProjectionTypeFound.Should().Be( projFound );
        result.Authenticated.Should().Be(authenticated);
    }

    [Theory]
    [InlineData("BingMaps", true, true)]
    [InlineData("OpenStreetMaps", true, true)]
    [InlineData("OpenTopoMaps", true, true)]
    [InlineData("GoogleMaps", true, true)]
    [InlineData("UnknownMaps", false, false)]
    public async Task CreateProjectionFromNameAsync(string projectionName, bool projCreated, bool authenticated )
    {
        var factory = J4JDeusEx.ServiceProvider.GetService<ProjectionFactory>();
        factory.Should().NotBeNull();

        var result = await factory!.CreateProjectionAsync(projectionName);
        result.ProjectionTypeFound.Should().Be(projCreated);
        result.Authenticated.Should().Be( authenticated );
    }

    [Theory]
    [InlineData(typeof(BingMapsProjection), true, true)]
    [InlineData(typeof(OpenStreetMapsProjection), true, true)]
    [InlineData(typeof(OpenTopoMapsProjection), true, true)]
    [InlineData(typeof(GoogleMapsProjection), true, true)]
    [InlineData(typeof(string), false, false)]
    public void CreateProjectionFromType(Type projType, bool projFound, bool authenticated)
    {
        var factory = J4JDeusEx.ServiceProvider.GetService<ProjectionFactory>();
        factory.Should().NotBeNull();

        var result = factory!.CreateProjection(projType);
        result.ProjectionTypeFound.Should().Be(projFound);
        result.Authenticated.Should().Be(authenticated);
    }

    [Theory]
    [InlineData(typeof(BingMapsProjection), true, true)]
    [InlineData(typeof(OpenStreetMapsProjection), true, true)]
    [InlineData(typeof(OpenTopoMapsProjection), true, true)]
    [InlineData(typeof(GoogleMapsProjection), true, true)]
    [InlineData(typeof(string), false, false)]
    public async Task CreateProjectionFromTypeAsync(Type projType, bool projCreated, bool authenticated)
    {
        var factory = J4JDeusEx.ServiceProvider.GetService<ProjectionFactory>();
        factory.Should().NotBeNull();

        var result = await factory!.CreateProjectionAsync(projType);
        result.ProjectionTypeFound.Should().Be(projCreated);
        result.Authenticated.Should().Be(authenticated);
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
        projection!.ImageFileExtension.Should().BeEquivalentTo( fileExtension );
    }

}
