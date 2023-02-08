using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Reflection;
using System.Threading;
using Autofac.Core;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;

namespace J4JMapLibrary;

public partial class MapProjectionFactory
{
    private readonly Dictionary<string, SourceConfigurationInfo> _sources = new(StringComparer.OrdinalIgnoreCase);

    private readonly IJ4JLogger _logger;
    private readonly IProjectionCredentials? _projCredentials;

    public ReadOnlyCollection<Type> ProjectionTypes =>
        _sources.Select(x => x.Value.MapProjectionType)
            .ToList()
            .AsReadOnly();

    public ReadOnlyCollection<string> ProjectionNames =>
        _sources.Select(x => x.Key)
            .ToList()
            .AsReadOnly();

    public void Search(params Type[] types) => Search(types.Select(x => x.Assembly).ToArray());

    public void Search( params Assembly[] assemblies )
    {
        var expanded = assemblies.ToList();

        var stringAssembly = Assembly.GetAssembly( typeof( string ) );
        if( stringAssembly != null )
            expanded.Add( stringAssembly );

        var toScan = expanded.SelectMany( x => x.DefinedTypes ).ToList();

        // find all the MapProjection types decorated with MapProjectionAttribute with public constructors
        var publicProj = new HasPublicConstructors<IMapProjection>();
        var decoratedProj = new DecoratedTypeTester<IMapProjection>( false, typeof( MapProjectionAttribute ) );

        var projTypes = toScan.Where( x => x.GetInterface( nameof( IMapProjection ) ) != null
                                       && publicProj.MeetsRequirements( x )
                                       && decoratedProj.MeetsRequirements( x ) )
                              .Select( x => new ProjectionTypeInfo( x ) )
                              .ToList();

        // find all the MapServer types decorated with MapServerAttribute with public constructors
        var publicServer = new HasPublicConstructors<IMapServer>();
        var decoratedServer = new DecoratedTypeTester<IMapServer>( false, typeof( MapServerAttribute ) );

        var serverTypes = toScan.Where( x => x.GetInterface( nameof( IMapServer ) ) != null
                                         && publicServer.MeetsRequirements( x )
                                         && decoratedServer.MeetsRequirements( x )
                                         && x.GetConstructors().Any( y => y.GetParameters().Length == 0 ) )
                                .Select( x => new ServerTypeInfo( x ) )
                                .ToList();

        foreach( var projType in projTypes )
        {
            // to be usable, a projType has to refer to a known serverType,
            // and the serverType must use a known credential type.
            // it must also have a name
            if( projType.ServerType == null || string.IsNullOrEmpty( projType.Name ) )
                continue;

            var serverType = serverTypes.FirstOrDefault( x => x.ServerType == projType.ServerType );
            if( serverType == null )
                continue;

            var credentialType = toScan.FirstOrDefault( x => x == serverType.CredentialType );
            if( credentialType == null )
                continue;

            // ensure the credentialType as a public parameterless constructor
            if( credentialType.GetConstructors().All( x => x.GetParameters().Length != 0 ) )
                continue;

            var serverCtor = new ConstructorParameterTester<IMapProjection>( serverType.ServerType,
                                                                             typeof( IJ4JLogger ),
                                                                             typeof( ITileCache ) );

            var credentialCtor = new ConstructorParameterTester<IMapProjection>( typeof( IProjectionCredentials ),
                serverType.ServerType,
                typeof( IJ4JLogger ),
                typeof( ITileCache ) );

            var hasServerCtor = serverCtor.MeetsRequirements( projType.ProjectionType );
            var hasCredentialsCtor = credentialCtor.MeetsRequirements( projType.ProjectionType );

            if( !hasServerCtor && !hasCredentialsCtor )
                continue;

            _sources.Add( projType.Name,
                          new SourceConfigurationInfo( projType.Name,
                                                       hasCredentialsCtor,
                                                       projType.ProjectionType,
                                                       serverType.ServerType,
                                                       credentialType ) );
        }
    }

    public async Task<TProj?> CreateMapProjection<TProj, TServer, TAuth>(
        CancellationToken cancellationToken = default,
        TAuth? credentials = null,
        TServer? mapServer = null,
        ITileCache? tileCache = null
    )
        where TProj : class, IMapProjection
        where TAuth : class
        where TServer : class, IMapServer, new()
    {
        var ctorInfo = _sources.Values
                               .FirstOrDefault( x => x.MapProjectionType == typeof( TProj ) );

        if( ctorInfo == null )
        {
            _logger.Error( "{0} is not a known map projection type", typeof( TProj ) );
            return null;
        }

        mapServer ??= new TServer();

        var retVal = credentials == null
            ? await CreateLookupCredentials( ctorInfo, mapServer, tileCache, cancellationToken )
            : await CreateWithSuppliedCredentials( ctorInfo,
                                                                 mapServer,
                                                                 tileCache,
                                                                 credentials,
                                                                 cancellationToken );
        switch( retVal )
        {
            case null:
                return null;

            case TProj castRetVal:
                return castRetVal;

            default:
                _logger.Error( "Could not cast {0} to {1}", retVal.GetType(), typeof( TProj ) );
                return null;
        }
    }

    public async Task<IMapProjection?> CreateMapProjection(
        Type projectionType,
        IMapServer? mapServer = null,
        object? credentials = null,
        ITileCache? tileCache = null,
        CancellationToken cancellationToken = default
    )
    {
        var ctorInfo = _sources.Values
                               .FirstOrDefault(x => x.MapProjectionType == projectionType);

        if (ctorInfo == null)
        {
            _logger.Error("{0} is not a known map projection type", projectionType);
            return null;
        }

        mapServer ??= Activator.CreateInstance(ctorInfo.ServerType) as IMapServer;

        if( mapServer == null )
        {
            _logger.Error("Could not create an instance of {0}", ctorInfo.ServerType);
            return null;
        }

        var retVal = credentials == null
            ? await CreateLookupCredentials(ctorInfo, mapServer, tileCache, cancellationToken)
            : await CreateWithSuppliedCredentials(ctorInfo,
                                                  mapServer,
                                                  tileCache,
                                                  credentials,
                                                  cancellationToken);

        return retVal;
    }
}