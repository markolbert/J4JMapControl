using FluentAssertions;
using J4JMapLibrary;
using J4JSoftware.DependencyInjection;
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

        var config = J4JDeusEx.ServiceProvider.GetRequiredService<ILibraryConfiguration>();
        config.Should().NotBeNull();
        config.ValidateConfiguration().Should().BeTrue();
        Configuration = config;
    }

    protected IJ4JLogger Logger { get; }
    protected ILibraryConfiguration Configuration { get; }

    protected async Task<BingMapProjection> GetBingMapProjection()
    {
        var key = Configuration.Credentials
                               .FirstOrDefault(x => x.Name.Equals("Bing",
                                                                  StringComparison.OrdinalIgnoreCase))
                              ?.Key;

        key.Should().NotBeNull().And.NotBeEmpty();

        var retVal = J4JDeusEx.ServiceProvider.GetService<BingMapProjection>();
        retVal.Should().NotBeNull();
        
        var okay = await retVal!.InitializeAsync( key!, BingMapType.Aerial );
        okay.Should().BeTrue();

        return retVal;
    }
}
