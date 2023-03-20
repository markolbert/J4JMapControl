using FluentAssertions;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.DeusEx;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace MapLibTests;

public class TestBase
{
    public static string[] ProjectionNames =
        new[] { "BingMaps", "OpenStreetMaps", "OpenTopoMaps", "GoogleMaps" };

    private readonly IConfiguration _config;

    protected TestBase()
    {
        var deusEx = new DeusEx();
        deusEx.Initialize().Should().BeTrue();

        var logger = J4JDeusEx.ServiceProvider.GetRequiredService<ILogger>();
        logger.Should().NotBeNull();
        Logger = logger;

        _config = J4JDeusEx.ServiceProvider.GetRequiredService<IConfiguration>();
    }

    protected ILogger Logger { get; }

    protected async Task<IProjection?> CreateProjection(
        string projName,
        ITileCache? cache = null,
        object? credentials = null
    )
    {
        if( credentials != null )
            return await CreateProjectionWithSuppliedCredentials( projName, cache, credentials);

        return await CreateProjectionUsingBuiltInCredentials(projName, cache);
    }

    private async Task<IProjection?> CreateProjectionWithSuppliedCredentials(
        string projName,
        ITileCache? cache,
        object credentials
    )
    {
        var retVal = projName switch
        {
            "BingMaps" => (IProjection) new BingMapsProjection(Logger, cache),
            "OpenStreetMaps" => new OpenStreetMapsProjection(Logger, cache),
            "OpenTopoMaps" => new OpenTopoMapsProjection(Logger, cache),
            "GoogleMaps" => new GoogleMapsProjection(Logger),
            _ => null
        };

        if (retVal == null)
        {
            Logger.Error("Unknown projection {0}", projName);
            return null;
        }

        var authenticated = await retVal.SetCredentialsAsync(credentials);
        authenticated.Should().BeTrue();

        return retVal;
    }

    private async Task<IProjection?> CreateProjectionUsingBuiltInCredentials( string projName, ITileCache? cache )
    {
        var retVal = projName switch
        {
            "BingMaps" => (IProjection) new BingMapsProjection( Logger, cache ),
            "OpenStreetMaps" => new OpenStreetMapsProjection( Logger, cache ),
            "OpenTopoMaps" => new OpenTopoMapsProjection( Logger, cache ),
            "GoogleMaps" => new GoogleMapsProjection( Logger ),
            _ => null
        };

        if( retVal == null )
        {
            Logger.Error("Unknown projection {0}", projName);
            return null;
        }

        var credentials = GetCredentials( projName );
        if( credentials == null )
            return null;

        var authenticated = await retVal.SetCredentialsAsync( credentials );
        authenticated.Should().BeTrue();

        return retVal;
    }

    protected object? GetCredentials( string credentialName )
    {
        var retVal = credentialName switch
        {
            "BingMaps" => (object)new BingCredentials(),
            "OpenStreetMaps" => new OpenStreetCredentials(),
            "OpenTopoMaps" => new OpenTopoCredentials(),
            "GoogleMaps" => new GoogleCredentials(),
            _ => null
        };

        if (retVal == null)
        {
            Logger.Error("Unknown credentials type {0}", credentialName);
            return null;
        }

        var section = _config.GetSection($"Credentials:{credentialName}");
        section.Bind(retVal);

        return retVal;
    }

    protected string GetCheckImagesFolder(string projectionName)
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
