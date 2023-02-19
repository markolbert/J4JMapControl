using FluentAssertions;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace MapLibTests;

public class TestBase
{
    public static string[] ProjectionNames =
        new[] { "BingMaps", "OpenStreetMaps", "OpenTopoMaps", "GoogleMaps" };

    protected TestBase()
    {
        var deusEx = new DeusEx();
        deusEx.Initialize().Should().BeTrue();

        var logger = J4JDeusEx.ServiceProvider.GetRequiredService<IJ4JLogger>();
        logger.Should().NotBeNull();
        Logger = logger;

        var config = J4JDeusEx.ServiceProvider.GetRequiredService<IProjectionCredentials>();
        config.Should().NotBeNull();
        Credentials = config;
    }

    protected IJ4JLogger Logger { get; }
    protected IProjectionCredentials Credentials { get; }

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

        if( retVal == null )
            return null;

        await retVal.AuthenticateAsync( credentials );
        return retVal;
    }

    private async Task<IProjection?> CreateProjectionUsingBuiltInCredentials( string projName, ITileCache? cache )
    {
        var retVal = projName switch
        {
            "BingMaps" => (IProjection) new BingMapsProjection( Credentials, Logger, cache ),
            "OpenStreetMaps" => new OpenStreetMapsProjection( Credentials, Logger, cache ),
            "OpenTopoMaps" => new OpenTopoMapsProjection( Credentials, Logger, cache ),
            "GoogleMaps" => new GoogleMapsProjection( Credentials, Logger ),
            _ => null
        };

        if( retVal == null )
            return null;

        await retVal.AuthenticateAsync( null );
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
