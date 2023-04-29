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
        Instance = new MapControlViewModelLocator( config, loggerFactory, inclDefaults );
        Instance.ProjectionFactory.ScanAssemblies( assembliesToSearch );
        Instance.ProjectionFactory.InitializeFactory();
    }

    private MapControlViewModelLocator(
        IConfiguration config,
        ILoggerFactory? loggerFactory = null,
        bool inclDefaults = true
        )
    {
        LoggerFactory = loggerFactory;
        ProjectionFactory = new ProjectionFactory( config, loggerFactory, inclDefaults );
    }

    public ProjectionFactory ProjectionFactory { get; }
    public ILoggerFactory? LoggerFactory { get; }
}
