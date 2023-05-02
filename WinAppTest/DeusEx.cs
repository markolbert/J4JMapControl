using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
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
    private J4JHostConfiguration? _hostConfiguration;
    private AppConfiguration? _appConfig;

    protected override J4JHostConfiguration GetHostConfiguration()
    {
        _hostConfiguration = new J4JWinAppHostConfiguration()
              .Publisher( "J4JSoftware" )
              .ApplicationName( "Test.WinMapLibrary" )
              .AddConfigurationInitializers( SetupConfiguration )
              .AddDependencyInjectionInitializers( SetupDependencyInjection )
              .LoggerInitializer( SetupLogger );

        return _hostConfiguration;
    }

    private void SetupConfiguration( IConfigurationBuilder builder )
    {
        // we pare the config file here because we need to load its contents
        // into the IConfiguration system, and we must decrypt its contents
        // before doing so
        _appConfig = ParseUserConfigFile( builder );
    }

    private AppConfiguration? ParseUserConfigFile( IConfigurationBuilder builder )
    {
        if ( !GetHostConfiguration().TryGetUserConfigurationFolder( out var userFolder ) )
            return null;

        // create a default version of AppConfiguration so we'll know
        // the location where the user config file should be stored
        var retVal = new AppConfiguration();

        var path = Path.Combine( userFolder!, "userConfig.json" );
        retVal.UserConfigurationFilePath = path;
        retVal.UserConfigurationFileExists = File.Exists( path );

        try
        {
            var encrypted = JsonSerializer.Deserialize<AppConfiguration>(File.ReadAllText(path));

            if( encrypted == null )
            {
                _hostConfiguration!.Logger.Error( "Could not parse user config file '{path}'", path );
                return retVal;
            }

            // recreate AppConfiguration, this time based on the contents
            retVal = encrypted.Decrypt(_hostConfiguration!.DataProtector);
            retVal.AddToConfiguration(builder);
            retVal.UserConfigurationFilePath = path;
            retVal.UserConfigurationFileExists = true;
        }
        catch (CryptographicException crypto)
        {
            _hostConfiguration!.Logger.Error("Cryptographic error '{mesg}', deleting user configuration file",
                                             crypto.Message);

            File.Delete(path);
        }
        catch (Exception ex)
        {
            _hostConfiguration!.Logger.Error("Problem parsing user configuration file '{file}', message was '{mesg}'",
                                             path,
                                             ex.Message);
        }

        return retVal;
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
        builder.Register( _ => _appConfig ?? new AppConfiguration() )
               .AsSelf()
               .SingleInstance();

        builder.Register(c =>
                {
                    var retVal = new ProjectionFactory( c.Resolve<ILoggerFactory>() );

                    retVal.InitializeFactory();

                    return retVal;
                })
               .AsSelf()
               .SingleInstance();

        builder.Register(c =>
                {
                    var retVal = new CredentialsFactory( c.Resolve<IConfiguration>(), c.Resolve<ILoggerFactory>() );

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
