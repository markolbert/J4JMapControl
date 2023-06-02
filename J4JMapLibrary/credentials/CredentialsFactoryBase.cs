using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

public abstract class CredentialsFactoryBase : ICredentialsFactory
{
    private readonly bool _includeDefaults;
    private readonly List<Assembly> _assemblies = new();
    private readonly Dictionary<string, ICredentials> _credentials = new( StringComparer.OrdinalIgnoreCase );
    private readonly ILogger? _logger;

    protected CredentialsFactoryBase(
        ILoggerFactory? loggerFactory = null,
        bool includeDefaults = true
    )
    {
        _includeDefaults = includeDefaults;
        _logger = loggerFactory?.CreateLogger<CredentialsFactoryBase>();
    }

    public CredentialsFactoryBase ScanAssemblies( params Type[] types ) =>
        ScanAssemblies( types.Distinct().Select( t => t.Assembly ).ToArray() );

    public CredentialsFactoryBase ScanAssemblies( params Assembly[] assemblies )
    {
        _assemblies.AddRange( assemblies );
        return this;
    }

    public bool InitializeFactory()
    {
        if( _includeDefaults )
            _assemblies.Add( typeof( ProjectionFactory ).Assembly );
        if( _includeDefaults )
            _assemblies.Add( typeof( ProjectionFactory ).Assembly );

        var toScan = _assemblies.Distinct().ToList();

        ProcessCredentialTypes( toScan );

        return _credentials.Any();
    }

    private void ProcessCredentialTypes( IEnumerable<Assembly> toScan )
    {
        var types = toScan
                   .SelectMany( a => a.GetTypes()
                                      .Where( t => !t.IsAbstract
                                               && t.IsAssignableTo( typeof( ICredentials ) )
                                               && t.GetConstructors()
                                                   .Any( c => c.GetParameters().Length == 0 ) ) )
                   .ToList();

        if( !types.Any() )
        {
            _logger?.LogWarning( "No map credentials types found" );
            return;
        }

        _credentials.Clear();

        foreach( var t in types )
        {
            var credentials = CreateCredentials( t );

            if( credentials == null )
            {
                _logger?.LogError( "Could not create instance of credentials type {type}", t );
                continue;
            }

            if( !_credentials.TryAdd( credentials.ProjectionName, credentials ) )
            {
                _logger?.LogWarning( "Multiple credential objects found for {projName}, ignoring all but first",
                                     credentials.ProjectionName );
            }
        }
    }

    public ICredentials? this[ string projName ] =>
        !_credentials.TryGetValue( projName, out var retVal ) ? null : retVal;

    protected abstract ICredentials? CreateCredentials( Type credType );

    public ICredentials? this[ Type projType ]
    {
        get
        {
            var kvp = _credentials.FirstOrDefault( x => x.Value.ProjectionType == projType );

            return string.IsNullOrEmpty( kvp.Key ) ? null : kvp.Value;
        }
    }
}
