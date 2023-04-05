using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

public class ProjectionFactory
{
    private readonly IConfiguration _config;
    private readonly List<Assembly> _assemblies = new();
    private readonly List<ProjectionTypeInfo> _projTypes = new();
    private readonly List<CredentialsTypeInfo> _credTypes = new();
    private readonly bool _includeDefaults;
    private readonly ILogger? _logger;
    private readonly ILoggerFactory? _loggerFactory;

    public ProjectionFactory(
        IConfiguration config,
        ILoggerFactory? loggerFactory = null,
        bool includeDefaults = true
    )
    {
        _config = config;

        _loggerFactory = loggerFactory;
        _logger = loggerFactory?.CreateLogger<ProjectionFactory>();

        _includeDefaults = includeDefaults;
    }

    public ProjectionFactory ScanAssemblies( params Type[] types ) =>
        ScanAssemblies( types.Distinct().Select( t => t.Assembly ).ToArray() );

    public ProjectionFactory ScanAssemblies() => ScanAssemblies( Enumerable.Empty<Assembly>().ToArray() );
    public ProjectionFactory ScanAssemblies( params Assembly[] assemblies )
    {
        _assemblies.AddRange( assemblies );
        return this;
    }

    public bool InitializeFactory()
    {
        if (_includeDefaults)
            _assemblies.Add(typeof(ProjectionFactory).Assembly);

        var toScan = _assemblies.Distinct().ToList();

        ProcessProjectionTypes(toScan);
        ProcessCredentialTypes(toScan);

        return _projTypes.Any();
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
                                                           p => p.ParameterType.IsAssignableTo( typeof( ILoggerFactory ) ) );
                                                   } )
                                              && t.GetInterface( typeof( IProjection ).FullName ?? string.Empty )
                                              != null ) )
                   .ToList();

        if( !types.Any() )
        {
            _logger?.LogWarning( "No IProjection types found" );
            return;
        }

        _projTypes.AddRange( types.Select( t => new ProjectionTypeInfo( t, _loggerFactory ) ) );
    }

    private void ProcessCredentialTypes( List<Assembly> toScan )
    {
        var types = toScan
                   .SelectMany( a => a.GetTypes()
                                      .Where( t => !t.IsAbstract
                                               && t.GetCustomAttribute<MapCredentialsAttribute>( false ) != null
                                               && t.GetConstructors()
                                                   .Any( c => c.GetParameters().Length == 0 ) ) )
                   .ToList();

        if( !types.Any() )
        {
            _logger?.LogWarning( "No map credentials types found" );
            return;
        }

        _credTypes.AddRange( types.Select( t => new CredentialsTypeInfo( t ) ) );
    }

    public ProjectionFactoryResult CreateProjection(
        string projName,
        ITileCache? cache = null,
        string? credentialsName = null,
        bool authenticate = true
    )
    {
        return Task.Run( async () =>
                             await CreateProjectionAsync( projName, cache, credentialsName, authenticate ) )
                   .Result;
    }

    public async Task<ProjectionFactoryResult> CreateProjectionAsync(
        string projName,
        ITileCache? cache = null,
        string? credentialsName = null,
        bool authenticate = true
    )
    {
        var projInfo = _projTypes
           .FirstOrDefault( x => x.Name.Equals( projName, StringComparison.OrdinalIgnoreCase ) );

        if( projInfo == null )
        {
            _logger?.LogError( "Could not find IProjection type named '{0}'", projName );
            return ProjectionFactoryResult.NotFound;
        }

        var retVal = CreateProjectionInternal( projInfo, cache );
        if( !retVal.ProjectionTypeFound || !authenticate )
            return retVal;

        var credentials = CreateCredentials( projInfo, credentialsName );
        if( credentials == null )
            return retVal;

        return retVal with { Authenticated = await retVal.Projection!.SetCredentialsAsync( credentials ) };
    }

    public ProjectionFactoryResult CreateProjection<TProj>(
        ITileCache? cache = null,
        string? credentialsName = null,
        bool authenticate = true
    )
        where TProj : IProjection =>
        CreateProjection( typeof( TProj ), cache, credentialsName, authenticate );

    public async Task<ProjectionFactoryResult> CreateProjectionAsync<TProj>(
        ITileCache? cache = null,
        string? credentialsName = null,
        bool authenticate = true
    )
        where TProj : IProjection =>
        await CreateProjectionAsync( typeof( TProj ),
                                     cache,
                                     credentialsName,
                                     authenticate );

    public ProjectionFactoryResult CreateProjection(
        Type projType,
        ITileCache? cache = null,
        string? credentialsName = null,
        bool authenticate = true
    ) =>
        Task.Run( async () =>
                      await CreateProjectionAsync( projType, cache, credentialsName, authenticate ) )
            .Result;

    public async Task<ProjectionFactoryResult> CreateProjectionAsync(
        Type projType,
        ITileCache? cache = null,
        string? credentialsName = null,
        bool authenticate = true
    )
    {
        if( !projType.IsAssignableTo( typeof( IProjection ) ) )
            return ProjectionFactoryResult.NotFound;

        var projInfo = _projTypes
           .FirstOrDefault( x => x.ProjectionType == projType );

        if( projInfo == null )
        {
            _logger?.LogError( "Could not find IProjection type '{0}'", projType );
            return ProjectionFactoryResult.NotFound;
        }

        var retVal = CreateProjectionInternal( projInfo, cache );
        if( !retVal.ProjectionTypeFound || !authenticate )
            return retVal;

        var credentials = CreateCredentials( projInfo, credentialsName );
        if( credentials == null )
            return retVal;

        return retVal with { Authenticated = await retVal.Projection!.SetCredentialsAsync( credentials ) };
    }

    private ProjectionFactoryResult CreateProjectionInternal(
        ProjectionTypeInfo projInfo,
        ITileCache? cache
    )
    {
        // create instance of IProjection and return
        IProjection? projection;

        // figure out the sequence of ctor parameters
        ProjectionCtorInfo? ctorInfo = null;

        switch( projInfo.ConstructorInfo.Count )
        {
            case 0:
                // no op
                break;

            case 1:
                ctorInfo = projInfo.ConstructorInfo.First();
                break;

            default:
                ctorInfo = projInfo.ConstructorInfo.FirstOrDefault( x => cache == null || x.SupportsCaching )
                 ?? projInfo.ConstructorInfo.First();
                break;
        }

        if( ctorInfo == null )
        {
            _logger?.LogError( "Could not find supported constructor for {0}", projInfo.ProjectionType );
            return ProjectionFactoryResult.NotFound;
        }

        var args = new object?[ ctorInfo.ParameterTypes.Count ];

        for( var idx = 0; idx < ctorInfo.ParameterTypes.Count; idx++ )
        {
            args[ idx ] = ctorInfo.ParameterTypes[ idx ] switch
            {
                ProjectionCtorParameterType.Cache => cache,
                ProjectionCtorParameterType.LoggerFactory => _loggerFactory,
                _ => throw new InvalidEnumArgumentException(
                    $"Unsupported {typeof( ProjectionCtorParameterType )} value '{ctorInfo.ParameterTypes[ idx ]}'" )
            };
        }

        try
        {
            projection = (IProjection) Activator.CreateInstance( projInfo.ProjectionType, args )!;
        }
        catch( Exception ex )
        {
            _logger?.LogError( "Could not create instance of {0}, message was {1}", projInfo.ProjectionType, ex.Message );
            return ProjectionFactoryResult.NotFound;
        }

        return new ProjectionFactoryResult( projection, true );
    }

    private object? CreateCredentials( ProjectionTypeInfo projInfo, string? credentialsName )
    {
        var credTypes = _credTypes
                       .Where( x => x.ProjectionType == projInfo.ProjectionType )
                       .ToList();

        var credType = string.IsNullOrEmpty( credentialsName )
            ? credTypes.FirstOrDefault()
            : credTypes.FirstOrDefault( x => x.Name.Equals( credentialsName, StringComparison.OrdinalIgnoreCase ) );

        // it's possible the search on serverName failed, so we need a fallback
        credType ??= credTypes.FirstOrDefault();

        if( credType == null )
        {
            if( string.IsNullOrEmpty( credentialsName ) )
                _logger?.LogWarning( "No valid credentials type supporting '{0}' projection found", projInfo.Name );
            else
            {
                _logger?.LogWarning(
                    "No credentials type named '{0}' found, and no other credentials supporting '{1}' projection found",
                    credentialsName,
                    projInfo.Name );
            }
        }

        if( credTypes.Count > 1 )
        {
            _logger?.LogWarning(
                "Multiple credentials types supporting '{0}' projection found, using {1} credentials type",
                projInfo.Name,
                credType?.Name ?? "Default" );
        }

        if( credType == null )
        {
            _logger?.LogError( "Could not find a credentials type supporting projection {0}", projInfo.Name );
            return null;
        }

        try
        {
            var retVal = Activator.CreateInstance( credType.CredentialsType )!;

            var section = _config.GetSection( $"Credentials:{credType.Name}" );
            section.Bind( retVal );

            return retVal;
        }
        catch( Exception ex )
        {
            _logger?.LogError( "Could not create an instance of credentials type {0}, message was {1}",
                            credType.CredentialsType,
                            ex.Message );
            return null;
        }
    }
}
