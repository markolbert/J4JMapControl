using FluentAssertions;
using J4JMapLibrary;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace MapLibTests;

public class TestBase
{
    protected TestBase()
    {
        var deusEx = new DeusEx();
        deusEx.Initialize().Should().BeTrue();

        var logger = J4JDeusEx.ServiceProvider.GetRequiredService<IJ4JLogger>();
        logger.Should().NotBeNull();
        Logger = logger;

        var config = J4JDeusEx.ServiceProvider.GetRequiredService<IProjectionCredentials>();
        config.Should().NotBeNull();
        Configuration = config;
    }

    protected IJ4JLogger Logger { get; }
    protected IProjectionCredentials Configuration { get; }

    protected ProjectionFactory GetFactory()
    {
        var retVal = J4JDeusEx.ServiceProvider.GetService<ProjectionFactory>();
        retVal.Should().NotBeNull();
        retVal!.Initialize();

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
