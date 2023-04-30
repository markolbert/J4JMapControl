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
    public void CreateProjectionFromName( string projectionName, bool projCreated, bool authenticated )
    {
        var projection = ProjectionFactory.CreateProjection( projectionName );

        if( !projCreated )
        {
            projection.Should().BeNull();
            return;
        }

        projection.Should().NotBeNull();

        var credentials = CredentialsFactory[ projectionName, true ];
        credentials.Should().NotBeNull();

        projection!.SetCredentials( credentials! ).Should().Be( authenticated );

        if( authenticated )
            projection.Authenticate().Should().BeTrue();
    }

    [Theory]
    [InlineData("BingMaps", true, true)]
    [InlineData("OpenStreetMaps", true, true)]
    [InlineData("OpenTopoMaps", true, true)]
    [InlineData("GoogleMaps", true, true)]
    [InlineData("UnknownMaps", false, false)]
    public async Task CreateProjectionFromNameAsync(string projectionName, bool projCreated, bool authenticated )
    {
        var projection = ProjectionFactory.CreateProjection(projectionName);

        if (!projCreated)
        {
            projection.Should().BeNull();
            return;
        }

        projection.Should().NotBeNull();

        var credentials = CredentialsFactory[projectionName, true];
        credentials.Should().NotBeNull();

        projection!.SetCredentials(credentials!).Should().Be(authenticated);

        if( !authenticated )
            return;

        var authResult = await projection.AuthenticateAsync();
        authResult.Should().Be( authenticated );
    }

    [Theory]
    [InlineData(typeof(BingMapsProjection), true, true)]
    [InlineData(typeof(OpenStreetMapsProjection), true, true)]
    [InlineData(typeof(OpenTopoMapsProjection), true, true)]
    [InlineData(typeof(GoogleMapsProjection), true, true)]
    [InlineData(typeof(string), false, false)]
    public void CreateProjectionFromType(Type projType, bool projCreated, bool authenticated)
    {
        var projection = ProjectionFactory.CreateProjection(projType);

        if (!projCreated)
        {
            projection.Should().BeNull();
            return;
        }

        projection.Should().NotBeNull();

        var credentials = CredentialsFactory[projType, true];
        credentials.Should().NotBeNull();

        projection!.SetCredentials(credentials!).Should().Be(authenticated);

        if (authenticated)
            projection.Authenticate().Should().BeTrue();
    }

    [Theory]
    [InlineData(typeof(BingMapsProjection), true, true)]
    [InlineData(typeof(OpenStreetMapsProjection), true, true)]
    [InlineData(typeof(OpenTopoMapsProjection), true, true)]
    [InlineData(typeof(GoogleMapsProjection), true, true)]
    [InlineData(typeof(string), false, false)]
    public async Task CreateProjectionFromTypeAsync(Type projType, bool projCreated, bool authenticated)
    {
        var projection = ProjectionFactory.CreateProjection(projType);

        if (!projCreated)
        {
            projection.Should().BeNull();
            return;
        }

        projection.Should().NotBeNull();

        var credentials = CredentialsFactory[projType, true];
        credentials.Should().NotBeNull();

        projection!.SetCredentials(credentials!).Should().Be(authenticated);

        if (!authenticated)
            return;

        var authResult = await projection.AuthenticateAsync();
        authResult.Should().Be(authenticated);
    }

    [Theory]
    [InlineData("BingMaps",".jpg")]
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
