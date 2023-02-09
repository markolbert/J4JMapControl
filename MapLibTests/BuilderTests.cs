using FluentAssertions;
using J4JMapLibrary;
using J4JSoftware.DeusEx;
using Microsoft.Extensions.DependencyInjection;
using J4JMapLibrary.MapBuilder;

namespace MapLibTests;

public class BuilderTests : TestBase
{
    [ Fact ]
    public async Task BuilderFail()
    {
        var builder = J4JDeusEx.ServiceProvider.GetService<ProjectionBuilder>();
        builder.Should().NotBeNull();

        builder!.Buildable.Should().BeFalse();
        var result = await builder.Build();
        result.Projection.Should().BeNull();
    }

    [ Theory ]
    [ InlineData( typeof( BingMapsProjection ), 0, true ) ]
    [ InlineData( typeof( string ), 0, false ) ]
    [InlineData(typeof(BingMapsProjection), 100, true)]
    [InlineData(typeof(BingMapsProjection), -100, true)]
    [InlineData(typeof(OpenStreetMapsProjection), 0, true)]
    [InlineData(typeof(OpenStreetMapsProjection), 100, true)]
    [InlineData(typeof(OpenStreetMapsProjection), -100, true)]
    [InlineData(typeof(OpenTopoMapsProjection), 0, true)]
    [InlineData(typeof(OpenTopoMapsProjection), 100, true)]
    [InlineData(typeof(OpenTopoMapsProjection), -100, true)]
    public async Task BuilderTypeAuthenticate( Type projectionType, int latencyRequested, bool projValid )
    {
        var builder = J4JDeusEx.ServiceProvider.GetService<ProjectionBuilder>();
        builder.Should().NotBeNull();

        builder!.Projection( projectionType )
                .Authenticate()
                .RequestLatency( latencyRequested );

        builder!.Buildable.Should().BeTrue();

        var result = await builder.Build();

        if( projValid )
        {
            result.Projection.Should().NotBeNull();
            result.Authenticated.Should().BeTrue();
            result.Projection!.MapServer.MaxRequestLatency.Should().Be( latencyRequested );
        }
        else
        {
            result.Projection.Should().BeNull();
            result.Authenticated.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData("NullMaps", 0, false)]
    [InlineData("BingMaps", 0, true)]
    [InlineData("BingMaps", 100, true)]
    [InlineData("BingMaps", -100, true)]
    [InlineData("OpenStreetMaps", 0, true)]
    [InlineData("OpenStreetMaps", 100, true)]
    [InlineData("OpenStreetMaps", -100, true)]
    [InlineData("OpenTopoMaps", 0, true)]
    [InlineData("OpenTopoMaps", 100, true)]
    [InlineData("OpenTopoMaps", -100, true)]
    public async Task BuilderNameAuthenticate(string projectionName, int latencyRequested, bool projValid)
    {
        var builder = J4JDeusEx.ServiceProvider.GetService<ProjectionBuilder>();
        builder.Should().NotBeNull();

        builder!.Projection(projectionName)
                .Authenticate()
                .RequestLatency(latencyRequested);

        builder!.Buildable.Should().BeTrue();

        var result = await builder.Build();

        if (projValid)
        {
            result.Projection.Should().NotBeNull();
            result.Authenticated.Should().BeTrue();
            result.Projection!.MapServer.MaxRequestLatency.Should().Be(latencyRequested);
        }
        else
        {
            result.Projection.Should().BeNull();
            result.Authenticated.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData(typeof(BingMapsProjection), 0, true)]
    [InlineData(typeof(string), 0, false)]
    [InlineData(typeof(BingMapsProjection), 100, true)]
    [InlineData(typeof(BingMapsProjection), -100, true)]
    [InlineData(typeof(OpenStreetMapsProjection), 0, true)]
    [InlineData(typeof(OpenStreetMapsProjection), 100, true)]
    [InlineData(typeof(OpenStreetMapsProjection), -100, true)]
    [InlineData(typeof(OpenTopoMapsProjection), 0, true)]
    [InlineData(typeof(OpenTopoMapsProjection), 100, true)]
    [InlineData(typeof(OpenTopoMapsProjection), -100, true)]
    public async Task BuilderTypeSkipAuthenticate(Type projectionType, int latencyRequested, bool projValid)
    {
        var builder = J4JDeusEx.ServiceProvider.GetService<ProjectionBuilder>();
        builder.Should().NotBeNull();

        builder!.Projection(projectionType)
                .SkipAuthentication()
                .RequestLatency(latencyRequested);

        builder!.Buildable.Should().BeTrue();

        var result = await builder.Build();

        if (projValid)
        {
            result.Projection.Should().NotBeNull();
            result.Authenticated.Should().BeFalse();
            result.Projection!.MapServer.MaxRequestLatency.Should().Be(latencyRequested);
        }
        else
        {
            result.Projection.Should().BeNull();
            result.Authenticated.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData("NullMaps", 0, false)]
    [InlineData("BingMaps", 0, true)]
    [InlineData("BingMaps", 100, true)]
    [InlineData("BingMaps", -100, true)]
    [InlineData("OpenStreetMaps", 0, true)]
    [InlineData("OpenStreetMaps", 100, true)]
    [InlineData("OpenStreetMaps", -100, true)]
    [InlineData("OpenTopoMaps", 0, true)]
    [InlineData("OpenTopoMaps", 100, true)]
    [InlineData("OpenTopoMaps", -100, true)]
    public async Task BuilderNameSkipAuthenticate(string projectionName, int latencyRequested, bool projValid)
    {
        var builder = J4JDeusEx.ServiceProvider.GetService<ProjectionBuilder>();
        builder.Should().NotBeNull();

        builder!.Projection(projectionName)
                .SkipAuthentication()
                .RequestLatency(latencyRequested);

        builder!.Buildable.Should().BeTrue();

        var result = await builder.Build();

        if (projValid)
        {
            result.Projection.Should().NotBeNull();
            result.Authenticated.Should().BeFalse();
            result.Projection!.MapServer.MaxRequestLatency.Should().Be(latencyRequested);
        }
        else
        {
            result.Projection.Should().BeNull();
            result.Authenticated.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData("NullMaps", 0, false)]
    [InlineData("BingMaps", 0, true)]
    [InlineData("BingMaps", 100, true)]
    [InlineData("BingMaps", -100, true)]
    [InlineData("OpenStreetMaps", 0, true)]
    [InlineData("OpenStreetMaps", 100, true)]
    [InlineData("OpenStreetMaps", -100, true)]
    [InlineData("OpenTopoMaps", 0, true)]
    [InlineData("OpenTopoMaps", 100, true)]
    [InlineData("OpenTopoMaps", -100, true)]
    public async Task BuilderNameCredentials(string projectionName, int latencyRequested, bool projValid)
    {
        var builder = J4JDeusEx.ServiceProvider.GetService<ProjectionBuilder>();
        builder.Should().NotBeNull();
        builder!.Factory.ProjectionCredentials.Should().NotBeNull();

        var credentialsFound = builder.Factory
                                      .ProjectionCredentials!
                                      .TryGetCredential( projectionName, out var key );

        switch( projectionName )
        {
            case "BingMaps":
            case "OpenStreetMaps":
            case "OpenTopoMaps":
                credentialsFound.Should().BeTrue();
                break;

            default:
                credentialsFound.Should().BeFalse();
                break;
        }

        var credentials = projectionName switch
        {
            "BingMaps" => (object) new BingCredentials( key!, BingMapType.Aerial ),
            _ => key!
        };

        builder!.Projection( projectionName )
                .Authenticate()
                .Credentials( credentials )
                .RequestLatency( latencyRequested );

        builder!.Buildable.Should().BeTrue();

        var result = await builder.Build();

        if (projValid)
        {
            result.Projection.Should().NotBeNull();
            result.Authenticated.Should().BeTrue();
            result.Projection!.MapServer.MaxRequestLatency.Should().Be(latencyRequested);
        }
        else
        {
            result.Projection.Should().BeNull();
            result.Authenticated.Should().BeFalse();
        }
    }

}
