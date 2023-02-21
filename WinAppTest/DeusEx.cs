using System.IO;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.Logging;
using J4JSoftware.WindowsAppUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

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

    private void SetupLogger(
        IConfiguration config,
        J4JHostConfiguration hostConfig,
        J4JLoggerConfiguration loggerConfig
    )
    {
        var logFile = Path.Combine(hostConfig.ApplicationConfigurationFolder, "log.txt");

        loggerConfig.SerilogConfiguration
                    .WriteTo.Debug()
                    .WriteTo.File(logFile, rollingInterval: RollingInterval.Minute);
    }

    private void SetupDependencyInjection(HostBuilderContext hbc, ContainerBuilder builder)
    {
        builder.Register(_ => new ProjectionCredentials(hbc.Configuration))
               .As<IProjectionCredentials>()
               .SingleInstance();

        builder.RegisterType<MemoryCache>()
               .AsSelf();

        builder.RegisterType<FileSystemCache>()
               .AsSelf();
    }
}
