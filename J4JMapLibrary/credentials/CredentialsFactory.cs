using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

public class CredentialsFactory
{
    public const string RootCredentialsPropertyName = "J4JMapCredentials";

    private readonly IConfiguration _config;
    private readonly bool _includeDefaults;
    private readonly List<Assembly> _assemblies = new();
    private readonly Dictionary<string, ICredentials> _credentials = new( StringComparer.OrdinalIgnoreCase );
    private readonly ILogger? _logger;

    public CredentialsFactory(
        IConfiguration config,
        ILoggerFactory? loggerFactory = null,
        bool includeDefaults = true
    )
    {
        _config = config;
        _includeDefaults = includeDefaults;
        _logger = loggerFactory?.CreateLogger<CredentialsFactory>();
    }

    public CredentialsFactory ScanAssemblies(params Type[] types) =>
        ScanAssemblies(types.Distinct().Select(t => t.Assembly).ToArray());

    public CredentialsFactory ScanAssemblies(params Assembly[] assemblies)
    {
        _assemblies.AddRange(assemblies);
        return this;
    }

    public bool InitializeFactory()
    {
        if (_includeDefaults)
            _assemblies.Add(typeof(ProjectionFactory).Assembly);
        if (_includeDefaults)
            _assemblies.Add(typeof(ProjectionFactory).Assembly);

        var toScan = _assemblies.Distinct().ToList();

        ProcessCredentialTypes(toScan);

        return _credentials.Any();
    }

    private void ProcessCredentialTypes(List<Assembly> toScan)
    {
        var types = toScan
                   .SelectMany(a => a.GetTypes()
                                     .Where(t => !t.IsAbstract
                                             && t.IsAssignableTo(typeof(ICredentials))
                                             && t.GetConstructors()
                                                 .Any(c => c.GetParameters().Length == 0)))
                   .ToList();

        if (!types.Any())
        {
            _logger?.LogWarning("No map credentials types found");
            return;
        }

        _credentials.Clear();

        foreach (var t in types)
        {
            var credentials = (ICredentials)Activator.CreateInstance(t)!;

            if( !_credentials.TryAdd( credentials.ProjectionName, credentials ) )
                _logger?.LogWarning( "Multiple credential objects found for {projName}, ignoring all but first",
                                     credentials.ProjectionName );
        }
    }

    public ICredentials? this[ string projName, bool initializeFromConfig = true ]
    {
        get
        {
            if( !_credentials.TryGetValue( projName, out var retVal ) )
                return null;

            if( !initializeFromConfig )
                return retVal;

            var configSection = _config.GetSection($"{RootCredentialsPropertyName}:{projName}");
            configSection.Bind(retVal);

            return retVal;
        }
    }

    public ICredentials? this[Type projType, bool initializeFromConfig = true]
    {
        get
        {
            var kvp = _credentials.FirstOrDefault( x => x.Value.ProjectionType == projType );

            if( string.IsNullOrEmpty( kvp.Key ) )
                return null;

            if( !initializeFromConfig )
                return kvp.Value;

            var configSection = _config.GetSection( $"{RootCredentialsPropertyName}:{kvp.Key}" );
            configSection.Bind( kvp.Value );

            return kvp.Value;
        }
    }
}
