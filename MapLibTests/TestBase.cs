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

        var config = J4JDeusEx.ServiceProvider.GetRequiredService<ILibraryConfiguration>();
        config.Should().NotBeNull();
        config.ValidateConfiguration().Should().BeTrue();
        Configuration = config;
    }

    protected IJ4JLogger Logger { get; }
    protected ILibraryConfiguration Configuration { get; }

    protected MapProjectionFactory GetFactory( bool searchDefaults = true )
    {
        var retVal = J4JDeusEx.ServiceProvider.GetRequiredService<MapProjectionFactory>();
        retVal.Should().NotBeNull();

        if( searchDefaults )
            retVal.Search( typeof( MapProjectionFactory ) );

        return retVal;
    }
}
