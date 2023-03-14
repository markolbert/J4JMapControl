using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace MapLibTests;

internal partial class DeusEx : J4JDeusExHosted
{
    protected override J4JHostConfiguration GetHostConfiguration() =>
        new J4JHostConfiguration( AppEnvironment.Console )
           .Publisher("J4JSoftware"  )
           .ApplicationName("Test.MapLibrary")
           .AddApplicationConfigurationFile("libConfig.json")
           .AddConfigurationInitializers(SetupConfiguration)
           .AddDependencyInjectionInitializers( SetupDependencyInjection )
           .LoggerInitializer( SetupLogger );

    private void SetupConfiguration( IConfigurationBuilder builder )
    {
        builder.AddUserSecrets<DeusEx>();
    }

    private ILogger SetupLogger(
        IConfiguration config,
        J4JHostConfiguration hostConfig
    )
    {
        var logFile = Path.Combine( hostConfig.ApplicationConfigurationFolder, "log.txt" );

        return new LoggerConfiguration()
            .WriteTo.Debug()
            .WriteTo.File(logFile, rollingInterval: RollingInterval.Minute)
            .CreateLogger();
    }
}
