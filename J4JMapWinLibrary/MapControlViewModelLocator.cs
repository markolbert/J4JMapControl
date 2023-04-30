using System.Reflection;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.Configuration;
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

    public ProjectionFactory ProjectionFactory { get; }
    public CredentialsFactory CredentialsFactory { get; }
    public CredentialsDialogFactory CredentialsDialogFactory { get; }
    public ILoggerFactory? LoggerFactory { get; }
}
