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

    protected CancellationToken GetCancellationToken( string mapType )
    {
        var source = new CancellationTokenSource();

        if( Configuration.TryGetConfiguration( mapType, out var mapConfig ) )
            source.CancelAfter( mapConfig!.MaxRequestLatency );
        else
        {
            Logger.Warning<string>( "Could not retrieve MaxRequestLatency for {0}, defaulting to 500ms", mapType );
            source.CancelAfter( 500 );
        }

        return source.Token;
    }

    protected MapProjectionFactory GetFactory( bool searchDefaults = true )
    {
        var retVal = J4JDeusEx.ServiceProvider.GetRequiredService<MapProjectionFactory>();
        retVal.Should().NotBeNull();

        if( searchDefaults )
            retVal.Search( typeof( MapProjectionFactory ) );

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
