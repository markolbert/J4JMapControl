using System.Text.Json;
using Autofac;
using J4JMapLibrary;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MapLibTests;

internal class DeusEx : J4JDeusExHosted
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

    private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
    {
        builder.Register( c =>
                {
                    var config = c.Resolve<LibraryConfiguration>();

                    if( !config.TryGetConfiguration( "Bing", out var srcConfig ) )
                        throw new ApplicationException( "Could not find configuration information for Bing Maps" );

                    if( srcConfig is not DynamicConfiguration dynamicConfig )
                        throw new ApplicationException( "Bing Maps configuration information is invalid" );

                    return new BingMapProjection( dynamicConfig, c.Resolve<IJ4JLogger>() );
                } )
               .AsSelf()
               .SingleInstance();

        builder.Register( c =>
                {
                    LibraryConfiguration? config;

                    try
                    {
                        // this will ignore the SourceConfiguration entries because
                        // they're polymorphic, so we go back afterwards and add them
                        config = hbc.Configuration.Get<LibraryConfiguration>();
                    }
                    catch
                    {
                        config = new LibraryConfiguration();
                    }

                    var sourceIdx = 0;
                    var keyValuePairs = hbc.Configuration.AsEnumerable().ToList();

                    while( keyValuePairs.Any( x => x.Key.Equals( $"SourceConfigurations:{sourceIdx}" ) ) )
                    {
                        if( keyValuePairs.Any(
                               x => x.Key.Equals( $"SourceConfigurations:{sourceIdx}:MetadataRetrievalUrl" ) ) )
                        {
                            var dynamicConfig = new DynamicConfiguration();
                            hbc.Configuration.GetSection( $"SourceConfigurations:{sourceIdx}" ).Bind( dynamicConfig );
                            config!.SourceConfigurations.Add( dynamicConfig );
                        }
                        else
                        {
                            var staticConfig = new StaticConfiguration();
                            hbc.Configuration.GetSection($"SourceConfigurations:{sourceIdx}").Bind(staticConfig);
                            config!.SourceConfigurations.Add(staticConfig);
                        }

                        sourceIdx++;
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
