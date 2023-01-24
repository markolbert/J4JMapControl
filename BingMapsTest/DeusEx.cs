using Autofac;
using J4JMapLibrary;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BingMapsTest;

internal class DeusEx : J4JDeusExHosted
{
    protected override J4JHostConfiguration? GetHostConfiguration() =>
        new J4JHostConfiguration( AppEnvironment.Console )
           .Publisher("J4JSoftware"  )
           .ApplicationName("Test.MapLibrary")
           .AddConfigurationInitializers(SetupConfiguration)
           .AddDependencyInjectionInitializers( SetupDependencyInjection )
           .LoggerInitializer( SetupLogger );

    private void SetupConfiguration( IConfigurationBuilder builder )
    {
        builder.AddUserSecrets<DeusEx>();
    }

    private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
    {
        builder.RegisterType<BingMapProjection>();

        builder.Register( c =>
                {
                    Configuration? config;

                    try
                    {
                        config = hbc.Configuration.Get<Configuration>();
                    }
                    catch
                    {
                        config = new Configuration();
                    }

                    return config!;
                } )
               .AsSelf()
               .SingleInstance();
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
