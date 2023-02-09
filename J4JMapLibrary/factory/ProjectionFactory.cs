using System.Collections.ObjectModel;
using System.Reflection;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

///TODO: google maps doesn't support ITileCache, but the factory logic assumes ALL projections do!
public partial class ProjectionFactory
{
    private readonly List<Assembly> _assemblyList = new();
    private readonly List<Type> _credentialTypes = new();

    private readonly IJ4JLogger _logger;

    private readonly Dictionary<string, ProjectionInfo> _sources = new( StringComparer.OrdinalIgnoreCase );

    public ProjectionFactory(
        IProjectionCredentials projCredentials,
        IJ4JLogger logger
    )
        : this( logger )
    {
        ProjectionCredentials = projCredentials;
    }

    public ProjectionFactory(
        IJ4JLogger logger
    )
    {
        _assemblyList.Add( GetType().Assembly );
        _credentialTypes.Add( typeof( string ) );

        _logger = logger;
        _logger.SetLoggedType( GetType() );
    }

    public IProjectionCredentials? ProjectionCredentials { get; }

    public ReadOnlyCollection<Type> ProjectionTypes =>
        _sources.Select( x => x.Value.MapProjectionType )
                .ToList()
                .AsReadOnly();

    public ReadOnlyCollection<string> ProjectionNames =>
        _sources.Select( x => x.Key )
                .ToList()
                .AsReadOnly();

    public void AddAssemblies( params Assembly[] assemblies ) => _assemblyList.AddRange( assemblies );

    public bool AddCredentialTypes( params Type[] types )
    {
        var retVal = true;

        foreach( var type in types )
        {
            if( type is { IsClass: true, IsAbstract: false } )
                _credentialTypes.Add( type );
            else retVal = false;
        }

        return retVal;
    }

    public void Initialize()
    {
        var allTypes = _assemblyList.SelectMany( x => x.DefinedTypes ).ToList();
        allTypes.AddRange( _credentialTypes.Select( x => x.GetTypeInfo() ) );

        var projections = allTypes
                         .Where( x => x.GetInterface( nameof( IMapProjection ) ) != null )
                         .Select( x => new ProjectionTypeInfo( x ) )
                         .Where( x => x.BasicConstructor != null
                                  && !string.IsNullOrEmpty( x.Name )
                                  && x.ServerType != null )
                         .ToList();

        // find all the MapServer types decorated with MapServerAttribute with public parameterless constructors
        var servers = allTypes.Where( x => x.GetInterface( nameof( IMapServer ) ) != null )
                              .Select( x => new ServerTypeInfo( x ) )
                              .Where( x => x is { HasPublicParameterlessConstructor: true, CredentialType: {} } )
                              .ToList();

        foreach( var projInfo in projections )
        {
            // to be usable, a projType has to refer to a known serverType,
            // and the serverType must use a known credential type.
            // it must also have a name
            if( projInfo.ServerType == null || string.IsNullOrEmpty( projInfo.Name ) )
                continue;

            var serverInfo = servers.FirstOrDefault( x =>
                                                         x.ServerType.GetInterface( projInfo.ServerType.ToString() )
                                                      != null );
            if( serverInfo == null )
                continue;

            // ensure the serverType has a public parameterless constructor
            if( !serverInfo.HasPublicParameterlessConstructor )
                continue;

            var credentialType = allTypes.FirstOrDefault( x => x == serverInfo.CredentialType );
            if( credentialType == null )
                continue;

            if( projInfo.BasicConstructor == null && projInfo.ConfigurationCredentialConstructor == null )
                continue;

            _sources.Add( projInfo.Name,
                          new ProjectionInfo( projInfo.Name,
                                              projInfo.ProjectionType,
                                              serverInfo.ServerType,
                                              credentialType,
                                              projInfo.BasicConstructor,
                                              projInfo.ConfigurationCredentialConstructor ) );
        }
    }

    private record ProjectionInfo(
        string Name,
        Type MapProjectionType,
        Type ServerType,
        Type CredentialsType,
        List<ParameterInfo>? BaseConstructor,
        List<ParameterInfo>? ConfigurationCredentialedConstructor
    );

    private enum ParameterType
    {
        TileCache,
        Credentials,
        MapServer,
        Logger,
        Other
    }

    private record ParameterValue( ParameterType Type, object? Value );

    private record ParameterInfo( int Position, ParameterType Type, bool Optional );
}
