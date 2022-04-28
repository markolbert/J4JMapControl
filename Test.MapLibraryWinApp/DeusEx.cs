using System;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.J4JMapControl;
using J4JSoftware.Logging;
using J4JSoftware.MapLibrary;
using J4JSoftware.WindowsAppUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Test.MapLibraryWinApp;

public class DeusEx : J4JDeusExWinApp
{
    protected override J4JHostConfiguration? GetHostConfiguration()
    {
        return new J4JWinAppHostConfiguration()
              .ApplicationName( "Test.MapLibraryWinApp" )
              .Publisher( "Jump for Joy Software" )
              .AddConfigurationInitializers( InitializeConfiguration )
              .LoggerInitializer(InitializeLogger)
              .AddDependencyInjectionInitializers(InitializeDependencyInjection);
    }

    private void InitializeConfiguration( IConfigurationBuilder builder ) => builder.AddUserSecrets<DeusEx>();

    private void InitializeLogger(
        IConfiguration config,
        J4JHostConfiguration hostConfig,
        J4JLoggerConfiguration loggerConfig
    )
    {
        loggerConfig.SerilogConfiguration
                    .WriteTo
                    .File( path: "log.txt", rollingInterval: RollingInterval.Day );
    }

    private void InitializeDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
    {
        builder.Register( ( c, p ) => new ApplicationInfo( "J4JMapControl-Test" ) )
               .AsImplementedInterfaces()
               .SingleInstance();

        builder.Register( c => hbc.Configuration.Get<Configuration>() )
               .AsSelf()
               .SingleInstance();

        builder.RegisterType<MapContext>()
               .OnActivated( x =>
                {
                    var mapRetriever = x.Context.Resolve<OpenStreetMapsImageRetriever>();

                    x.Instance.MapImageRetriever = mapRetriever;
                    x.Instance.Zoom = new Zoom( mapRetriever.MapRetrieverInfo );
                })
               .AsImplementedInterfaces()
               .SingleInstance();

        builder.RegisterType<ViewModel>()
               .AsSelf()
               .SingleInstance();

        builder.RegisterType<OpenStreetMapsImageRetriever>()
               .AsImplementedInterfaces()
               .AsSelf()
               .SingleInstance();

        builder.RegisterType<OpenTopoMapsImageRetriever>()
               .AsImplementedInterfaces()
               .SingleInstance();

        foreach( var mapType in Enum.GetValues<BingMapsImageRetriever.BingMapType>() )
        {
            builder.Register( c =>
                    {
                        var appInfo = c.Resolve<IApplicationInfo>();
                        var config = c.Resolve<Configuration>();
                        var logger = c.Resolve<IJ4JLogger>();

                        return new BingMapsImageRetriever( config.BingMapsApiKey, mapType, appInfo, logger );
                    } )
                   .AsImplementedInterfaces()
                   .SingleInstance();
        }
    }
}