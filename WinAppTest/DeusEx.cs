using System.IO;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Serilog.ILogger;

namespace WinAppTest;

internal class DeusEx : J4JDeusExWinApp
{
    protected override J4JHostConfiguration GetHostConfiguration() =>
        new J4JWinAppHostConfiguration()
           .Publisher("J4JSoftware")
           .ApplicationName("Test.WinMapLibrary")
           .AddConfigurationInitializers(SetupConfiguration)
           .AddDependencyInjectionInitializers(SetupDependencyInjection)
           .LoggerInitializer(SetupLogger);

    private void SetupConfiguration(IConfigurationBuilder builder)
    {
        builder.AddUserSecrets<DeusEx>();
    }

    private ILogger SetupLogger(
        IConfiguration config,
        J4JHostConfiguration hostConfig
    )
    {
        var logFile = Path.Combine(hostConfig.ApplicationConfigurationFolder, "log.txt");

        return new LoggerConfiguration()
              .MinimumLevel.Verbose()
              .WriteTo.Debug()
              .WriteTo.File( logFile, rollingInterval: RollingInterval.Minute )
              .CreateLogger();
    }

    private void SetupDependencyInjection(HostBuilderContext hbc, ContainerBuilder builder)
    {
        builder.Register(c =>
                {
                    var retVal = new ProjectionFactory(c.Resolve<IConfiguration>(),
                                                       c.Resolve<ILoggerFactory>());

                    retVal.InitializeFactory();

                    return retVal;
                })
               .AsSelf()
               .SingleInstance();

        builder.RegisterType<MemoryCache>()
               .AsSelf();

        builder.RegisterType<FileSystemCache>()
               .AsSelf();
    }
}
