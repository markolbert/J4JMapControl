using FluentAssertions;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.DependencyInjection;

namespace MapLibTests;

public class CredentialsTest : TestBase
{
    [ Fact ]
    public void Bing()
    {
        var credentials = GetCredentials( "BingMaps" ) as BingCredentials;
        credentials.Should().NotBeNull();

        credentials!.ApiKey.Should().NotBeEmpty();
    }

    [Fact]
    public void Google()
    {
        var credentials = GetCredentials("GoogleMaps") as GoogleCredentials;
        credentials.Should().NotBeNull();

        credentials!.ApiKey.Should().NotBeEmpty();
        credentials!.SignatureSecret.Should().NotBeEmpty();
    }

    [Fact]
    public void OpenStreetMaps()
    {
        var credentials = GetCredentials( "OpenStreetMaps" ) as OpenStreetCredentials;
        credentials.Should().NotBeNull();

        credentials!.UserAgent.Should().NotBeEmpty();
    }

    [Fact]
    public void OpenTopoMaps()
    {
        var credentials = GetCredentials("OpenTopoMaps") as OpenTopoCredentials;
        credentials.Should().NotBeNull();

        credentials!.UserAgent.Should().NotBeEmpty();
    }

}
