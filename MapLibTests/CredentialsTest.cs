using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
        var credentials = J4JDeusEx.ServiceProvider.GetService<BingCredentials>();
        credentials.Should().NotBeNull();

        credentials!.ApiKey.Should().NotBeEmpty();
    }

    [Fact]
    public void Google()
    {
        var credentials = J4JDeusEx.ServiceProvider.GetService<GoogleCredentials>();
        credentials.Should().NotBeNull();

        credentials!.ApiKey.Should().NotBeEmpty();
        credentials!.SignatureSecret.Should().NotBeEmpty();
    }

    [Fact]
    public void OpenStreetMaps()
    {
        var credentials = J4JDeusEx.ServiceProvider.GetService<OpenStreetCredentials>();
        credentials.Should().NotBeNull();

        credentials!.UserAgent.Should().NotBeEmpty();
    }

    [Fact]
    public void OpenTopoMaps()
    {
        var credentials = J4JDeusEx.ServiceProvider.GetService<OpenTopoCredentials>();
        credentials.Should().NotBeNull();

        credentials!.UserAgent.Should().NotBeEmpty();
    }

}
