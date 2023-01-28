using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace MapLibTests;

internal partial class DeusEx : J4JDeusExHosted
{
    protected override J4JHostConfiguration? GetHostConfiguration() =>
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

    private void SetupLogger(
        IConfiguration config,
        J4JHostConfiguration hostConfig,
        J4JLoggerConfiguration loggerConfig
    )
    {
        var logFile = Path.Combine( hostConfig.ApplicationConfigurationFolder, "log.txt" );

        loggerConfig.SerilogConfiguration
                    .WriteTo.Debug()
                    .WriteTo.File( logFile, rollingInterval: RollingInterval.Minute );
    }
}
