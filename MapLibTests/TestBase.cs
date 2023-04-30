using FluentAssertions;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.DeusEx;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MapLibTests;

public class TestBase
{
    public static readonly string[] ProjectionNames =
        new[] { "BingMaps", "OpenStreetMaps", "OpenTopoMaps", "GoogleMaps" };

    protected TestBase()
    {
        var deusEx = new DeusEx();
        deusEx.Initialize().Should().BeTrue();

        LoggerFactory = J4JDeusEx.ServiceProvider.GetRequiredService<ILoggerFactory>();
        LoggerFactory.Should().NotBeNull();
        Logger = LoggerFactory.CreateLogger( GetType() );

        ProjectionFactory = GetRequiredService<ProjectionFactory>();
        CredentialsFactory = GetRequiredService<CredentialsFactory>();
    }

    private T GetRequiredService<T>()
        where T : class
    {
        var service = J4JDeusEx.ServiceProvider.GetRequiredService<T>();
        service.Should().NotBeNull();

        return service;
    }

    protected ILoggerFactory LoggerFactory { get; }
    protected ILogger Logger { get; }

    protected ProjectionFactory ProjectionFactory { get; }
    protected CredentialsFactory CredentialsFactory { get; }

    protected async Task<IProjection?> CreateProjection(
        string projName,
        object? credentials = null
    )
    {
        credentials ??= CredentialsFactory[ projName, true ];
        credentials.Should().NotBeNull();

        var retVal = projName switch
        {
            "BingMaps" => (IProjection)new BingMapsProjection(LoggerFactory),
            "OpenStreetMaps" => new OpenStreetMapsProjection(LoggerFactory),
            "OpenTopoMaps" => new OpenTopoMapsProjection(LoggerFactory),
            "GoogleMaps" => new GoogleMapsProjection(LoggerFactory),
            _ => null
        };

        if (retVal == null)
        {
            Logger.LogError("Unknown projection {projName}", projName);
            return null;
        }

        retVal.SetCredentials(credentials!).Should().BeTrue();
        var authenticated = await retVal.AuthenticateAsync();
        authenticated.Should().BeTrue();

        return retVal;
    }

    protected static string GetCheckImagesFolder(string projectionName)
    {
        var retVal = Environment.CurrentDirectory;

        for (var idx = 0; idx < 3; idx++)
        {
            retVal = Path.GetDirectoryName(retVal)!;
        }

        retVal = Path.Combine(retVal, "check-images", projectionName);
        Directory.CreateDirectory(retVal);

        return retVal;
    }

}
