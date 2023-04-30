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
        var credentials = CredentialsFactory["BingMaps", true] as BingCredentials;
        credentials.Should().NotBeNull();

        credentials!.ApiKey.Should().NotBeEmpty();
    }

    [Fact]
    public void Google()
    {
        var credentials = CredentialsFactory["GoogleMaps", true] as GoogleCredentials;
        credentials.Should().NotBeNull();

        credentials!.ApiKey.Should().NotBeEmpty();
        credentials!.SignatureSecret.Should().NotBeEmpty();
    }

    [Fact]
    public void OpenStreetMaps()
    {
        var credentials = CredentialsFactory["OpenStreetMaps", true] as OpenStreetCredentials;
        credentials.Should().NotBeNull();

        credentials!.UserAgent.Should().NotBeEmpty();
    }

    [Fact]
    public void OpenTopoMaps()
    {
        var credentials = CredentialsFactory["OpenTopoMaps", true] as OpenTopoCredentials;
        credentials.Should().NotBeNull();

        credentials!.UserAgent.Should().NotBeEmpty();
    }

}
