using System;
using System.Reflection;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapWinLibrary;

public class MapControlViewModelLocator
{
    public static MapControlViewModelLocator? Instance { get; private set; }

    public static void Initialize(
        IConfiguration config,
        ILoggerFactory? loggerFactory = null,
        bool inclDefaults = true,
        params Assembly[] assembliesToSearch
    )
    {
        Instance = new MapControlViewModelLocator(config, loggerFactory, inclDefaults, assembliesToSearch );
    }

    private MapControlViewModelLocator(
        IConfiguration config,
        ILoggerFactory? loggerFactory = null,
        bool inclDefaults = true,
        params Assembly[] assembliesToSearch
        )
    {
        LoggerFactory = loggerFactory;

        ProjectionFactory = new ProjectionFactory( loggerFactory, inclDefaults );
        ProjectionFactory.ScanAssemblies( assembliesToSearch );
        ProjectionFactory.InitializeFactory();

        CredentialsFactory = new CredentialsFactory( config, loggerFactory, inclDefaults );
        CredentialsFactory.ScanAssemblies( assembliesToSearch );
        CredentialsFactory.InitializeFactory();

        CredentialsDialogFactory = new CredentialsDialogFactory( loggerFactory, assembliesToSearch );
    }

    public static void Initialize( IServiceProvider svcProvider )
    {
        var loggerFactory = svcProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger<MapControlViewModelLocator>();

        var projFactory = svcProvider.GetService<ProjectionFactory>();
        if( projFactory == null )
            logger?.LogCritical( "Could not create {type}, aborting", typeof( ProjectionFactory ) );

        var credFactory = svcProvider.GetService<CredentialsFactory>();
        if( credFactory == null )
            logger?.LogCritical( "Could not create {type}, aborting", typeof( CredentialsFactory ) );

        var credDlgFactory = svcProvider.GetService<CredentialsDialogFactory>();
        if( credDlgFactory == null )
            logger?.LogCritical( "Could not create {type}, aborting", typeof( CredentialsDialogFactory ) );

        if( projFactory == null || credFactory == null || credDlgFactory == null )
            throw new ArgumentException( $"Could not create instance of {typeof( MapControlViewModelLocator )}" );

        Instance = new MapControlViewModelLocator( projFactory, credFactory, credDlgFactory, loggerFactory );
    }

    private MapControlViewModelLocator(
        ProjectionFactory projFactory,
        CredentialsFactory credFactory,
        CredentialsDialogFactory credDlgFactory,
        ILoggerFactory? loggerFactory
    )
    {
        ProjectionFactory = projFactory;
        CredentialsFactory = credFactory;
        CredentialsDialogFactory = credDlgFactory;
        LoggerFactory = loggerFactory;
    }

    public ProjectionFactory ProjectionFactory { get; }
    public CredentialsFactory CredentialsFactory { get; }
    public CredentialsDialogFactory CredentialsDialogFactory { get; }
    public ILoggerFactory? LoggerFactory { get; }
}
