using System.Reflection;
using J4JSoftware.Logging;

namespace J4JSoftware.J4JMapLibrary;

public class ProjectionFactory
{
    private readonly IJ4JLogger _logger;
    private readonly bool _includeDefaults;
    private readonly List<ProjectionTypeInfo> _projTypes = new();
    private readonly List<ServerTypeInfo> _serverTypes = new();
    private readonly List<CredentialsTypeInfo> _credentialTypes = new();

    public ProjectionFactory(
        IJ4JLogger logger,
        bool includeDefaults = true
    )
    {
        _logger = logger;
        _logger.SetLoggedType( GetType() );

        _includeDefaults = includeDefaults;
    }

    public void ScanAssemblies( params Type[] types ) =>
        ScanAssemblies( types.Distinct().Select( t => t.Assembly ).ToArray() );

    public void ScanAssemblies( params Assembly[] assemblies )
    {
        var toScan = assemblies.ToList();

        if( _includeDefaults )
            toScan.Add( typeof( ProjectionFactory ).Assembly );

        toScan = toScan.Distinct().ToList();

        ProcessProjectionTypes(toScan);
        ProcessServerTypes( toScan );
    }

    private void ProcessProjectionTypes( List<Assembly> toScan )
    {
        _projTypes.Clear();

        var types = toScan
                       .SelectMany( a =>
                                        a.GetTypes()
                                         .Where( t => !t.IsAbstract
                                                  && t.GetCustomAttribute<ProjectionAttribute>( false ) != null
                                                  && t.GetConstructors()
                                                      .Any( c =>
                                                       {
                                                           var ctorParams = c.GetParameters();

                                                           return ctorParams.Any(
                                                                   p => p.ParameterType.IsAssignableTo(
                                                                       typeof( IJ4JLogger ) ) )
                                                            && ctorParams.Any(
                                                                   p => p.ParameterType.GetInterface(
                                                                           typeof( IMapServer ).FullName
                                                                        ?? string.Empty )
                                                                    != null );
                                                       } )
                                                  && t.GetInterface( typeof( IProjection ).FullName ?? string.Empty )
                                                  != null ) )
                       .ToList();

        if( !types.Any() )
        {
            _logger.Warning("No IProjection types found");
            return;
        }

        _projTypes.AddRange(types.Select(t=>new ProjectionTypeInfo(t)));
    }

    private void ProcessServerTypes( List<Assembly> toScan )
    {
        var types = toScan
           .SelectMany(a => a.GetTypes()
                             .Where(t => !t.IsAbstract
                                     && t.GetCustomAttribute<MapServerAttribute>(false) != null
                                     && t.IsAssignableTo(typeof(IMapServer))
                                     && t.GetConstructors()
                                         .Any(c => c.GetParameters().Length == 0)))
                         .ToList();

        if( !types.Any() )
        {
            _logger.Warning( "No IMapServer types found" );
            return;
        }

        _serverTypes.AddRange(types.Select(t => new ServerTypeInfo(t)));
    }

    private void ProcessCredentialTypes(List<Assembly> toScan)
    {
        var types = toScan
                   .SelectMany(a => a.GetTypes()
                                     .Where(t => !t.IsAbstract
                                             && t.GetCustomAttribute<MapCredentialsAttribute>(false) != null
                                             && t.GetConstructors()
                                                 .Any(c => c.GetParameters().Length == 0)))
                   .ToList();

        if (!types.Any())
        {
            _logger.Warning("No map credentials types found");
            return;
        }

        _credentials.AddRange(types.Select(t => new ServerTypeInfo(t)));
    }

    public ProjectionFactoryResult CreateProjection( string projectionName, bool authenticate = true )
    {
        return ProjectionFactoryResult.NotFound;
    }

    public ProjectionFactoryResult CreateProjection<TProj>(bool authenticate = true)
        where TProj : IProjection =>
        CreateProjection( typeof( TProj ), authenticate );

    public ProjectionFactoryResult CreateProjection( Type type, bool authenticate = true)
    {
        if( !type.IsAssignableTo( typeof( IProjection ) ) )
            return ProjectionFactoryResult.NotFound;

        return ProjectionFactoryResult.NotFound;
    }
}
