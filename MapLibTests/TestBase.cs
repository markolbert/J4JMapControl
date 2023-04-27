using FluentAssertions;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.DeusEx;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MapLibTests;

public class TestBase
{
    public static readonly string[] ProjectionNames =
        new[] { "BingMaps", "OpenStreetMaps", "OpenTopoMaps", "GoogleMaps" };

    private readonly IConfiguration _config;

    protected TestBase()
    {
        var deusEx = new DeusEx();
        deusEx.Initialize().Should().BeTrue();

        LoggerFactory = J4JDeusEx.ServiceProvider.GetRequiredService<ILoggerFactory>();
        LoggerFactory.Should().NotBeNull();
        Logger = LoggerFactory.CreateLogger( GetType() );

        _config = J4JDeusEx.ServiceProvider.GetRequiredService<IConfiguration>();
    }

    protected ILoggerFactory LoggerFactory { get; }
    protected ILogger Logger { get; }

    protected async Task<IProjection?> CreateProjection(
        string projName,
        object? credentials = null
    )
    {
        if( credentials != null )
            return await CreateProjectionWithSuppliedCredentials( projName, credentials);

        return await CreateProjectionUsingDiscoveredCredentials(projName);
    }

    private async Task<IProjection?> CreateProjectionWithSuppliedCredentials(
        string projName,
        object credentials
    )
    {
        var retVal = projName switch
        {
            "BingMaps" => (IProjection) new BingMapsProjection(LoggerFactory),
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

        retVal.SetCredentials(credentials).Should().BeTrue();
        var authenticated = await retVal.AuthenticateAsync();
        authenticated.Should().BeTrue();

        return retVal;
    }

    private async Task<IProjection?> CreateProjectionUsingDiscoveredCredentials( string projName )
    {
        var retVal = projName switch
        {
            "BingMaps" => (IProjection) new BingMapsProjection( LoggerFactory ),
            "OpenStreetMaps" => new OpenStreetMapsProjection( LoggerFactory ),
            "OpenTopoMaps" => new OpenTopoMapsProjection( LoggerFactory ),
            "GoogleMaps" => new GoogleMapsProjection( LoggerFactory ),
            _ => null
        };

        if( retVal == null )
        {
            Logger.LogError("Unknown projection {projName}", projName);
            return null;
        }

        var credentials = GetCredentials( projName );
        if( credentials == null )
            return null;

        retVal.SetCredentials(credentials).Should().BeTrue();
        var authenticated = await retVal.AuthenticateAsync();
        authenticated.Should().BeTrue();

        return retVal;
    }

    protected object? GetCredentials( string projectionName )
    {
        var retVal = projectionName switch
        {
            "BingMaps" => (object)new BingCredentials(),
            "OpenStreetMaps" => new OpenStreetCredentials(),
            "OpenTopoMaps" => new OpenTopoCredentials(),
            "GoogleMaps" => new GoogleCredentials(),
            _ => null
        };

        if (retVal == null)
        {
            Logger.LogError("Unknown projection name {projName}", projectionName);
            return null;
        }

        var section = _config.GetSection($"Credentials:{projectionName}");
        section.Bind(retVal);

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
