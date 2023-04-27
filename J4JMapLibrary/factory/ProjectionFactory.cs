#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ProjectionFactory.cs
//
// This file is part of JumpForJoy Software's J4JMapLibrary.
// 
// J4JMapLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapLibrary. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

public class ProjectionFactory
{
    public event EventHandler<CredentialsNeededEventArgs>? CredentialsNeeded;

    private readonly IConfiguration _config;
    private readonly List<Assembly> _assemblies = new();
    private readonly List<ProjectionTypeInfo> _projTypes = new();
    private readonly List<ICredentials> _credentials = new();
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
            var credentials = (ICredentials) Activator.CreateInstance( t )!;

            var configSection = _config.GetSection( $"Credentials:{credentials.ProjectionName}" );
            configSection.Bind( credentials );

            _credentials.Add( (ICredentials) credentials );
        }
    }

    public IEnumerable<string> ProjectionNames => _projTypes.Select( x => x.Name );
    public IEnumerable<Type> ProjectionTypes => _projTypes.Select( x => x.ProjectionType );

    public bool HasProjection( string? projectionName ) =>
        _projTypes.Any( x => x.Name.Equals( projectionName, StringComparison.OrdinalIgnoreCase ) );
    public bool HasProjection<TProj>() => HasProjection( typeof( TProj ) );
    public bool HasProjection( Type projectionType ) => _projTypes.Any( x => x.ProjectionType == projectionType );

    public ReadOnlyCollection<ICredentials> Credentials => _credentials.AsReadOnly();

    public ProjectionFactoryResult CreateProjection(
        string projName,
        object? credentials = null,
        bool useDiscoveredCredentials = true,
        bool authenticate = true
    )
    {
        return Task.Run( async () =>
                             await CreateProjectionAsync( projName,
                                                          credentials,
                                                          useDiscoveredCredentials,
                                                          authenticate ) )
                   .Result;
    }

    public async Task<ProjectionFactoryResult> CreateProjectionAsync(
        string projName,
        object? credentials = null,
        bool useDiscoveredCredentials = true,
        bool authenticate = true,
        CancellationToken ctx = default
    )
    {
        var projInfo = _projTypes
           .FirstOrDefault( x => x.Name.Equals( projName, StringComparison.OrdinalIgnoreCase ) );

        if( projInfo == null )
        {
            _logger?.LogError( "Could not find IProjection type named '{projName}'", projName );
            return ProjectionFactoryResult.NotFound;
        }

        var projection = CreateProjectionInternal( projInfo );
        if( projection == null || !authenticate )
            return new ProjectionFactoryResult( projection, projection != null );

        if (!SetCredentials(projection, projInfo, credentials, useDiscoveredCredentials))
            return new ProjectionFactoryResult(projection, true);

        return new ProjectionFactoryResult(projection, true)
        {
            Authenticated = await projection.AuthenticateAsync(ctx)
        };
    }

    public ProjectionFactoryResult CreateProjection<TProj>(
        object? credentials = null,
        bool useDiscoveredCredentials = true,
        bool authenticate = true
    )
        where TProj : IProjection =>
        CreateProjection( typeof( TProj ), credentials, useDiscoveredCredentials, authenticate );

    public async Task<ProjectionFactoryResult> CreateProjectionAsync<TProj>(
        object? credentials = null,
        bool useDiscoveredCredentials = true,
        bool authenticate = true
    )
        where TProj : IProjection =>
        await CreateProjectionAsync( typeof( TProj ),
                                     credentials,
                                     useDiscoveredCredentials,
                                     authenticate );

    public ProjectionFactoryResult CreateProjection(
        Type projType,
        object? credentials = null,
        bool useDiscoveredCredentials = true,
        bool authenticate = true
    ) =>
        Task.Run( async () =>
                      await CreateProjectionAsync( projType,
                                                   credentials,
                                                   useDiscoveredCredentials,
                                                   authenticate ) )
            .Result;

    public async Task<ProjectionFactoryResult> CreateProjectionAsync(
        Type projType,
        object? credentials = null,
        bool useDiscoveredCredentials = true,
        bool authenticate = true,
        CancellationToken ctx = default
    )
    {
        if( !projType.IsAssignableTo( typeof( IProjection ) ) )
            return ProjectionFactoryResult.NotFound;

        var projInfo = _projTypes
           .FirstOrDefault( x => x.ProjectionType == projType );

        if( projInfo == null )
        {
            _logger?.LogError( "Could not find IProjection type '{projType}'", projType );
            return ProjectionFactoryResult.NotFound;
        }

        var projection = CreateProjectionInternal( projInfo );
        if( projection == null || !authenticate )
            return new ProjectionFactoryResult( projection, projection != null );

        if( !SetCredentials(projection,projInfo,credentials, useDiscoveredCredentials))
            return new ProjectionFactoryResult(projection, true);

        return new ProjectionFactoryResult(projection, true)
        {
            Authenticated = await projection.AuthenticateAsync(ctx)
        };
    }

    private IProjection? CreateProjectionInternal( ProjectionTypeInfo projInfo )
    {
        IProjection? projection;

        // figure out the sequence of ctor parameters
        var ctorInfo = projInfo.ConstructorInfo.Count == 1
            ? projInfo.ConstructorInfo.First()
            : null;

        if( ctorInfo == null )
        {
            _logger?.LogError( "Could not find supported constructor for {projType}", projInfo.ProjectionType );
            return null;
        }

        var args = new object?[ ctorInfo.ParameterTypes.Count ];

        for( var idx = 0; idx < ctorInfo.ParameterTypes.Count; idx++ )
        {
            args[ idx ] = ctorInfo.ParameterTypes[ idx ] switch
            {
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
            _logger?.LogError( "Could not create instance of {projType}, message was {mesg}",
                               projInfo.ProjectionType,
                               ex.Message );

            return null;
        }

        return projection;
    }

    private bool SetCredentials(
        IProjection projection,
        ProjectionTypeInfo projInfo,
        object? credentials,
        bool useDiscoveredCredentials
    )
    {
        var cancelOnFailure = false;
        var retVal = false;

        if( credentials == null && useDiscoveredCredentials )
        {
            var projType = projection.GetType();

            var knownCredentials = _credentials.Where( x => x.ProjectionType == projType )
                                               .ToList();

            if( knownCredentials.Any() )
                credentials = knownCredentials[0];

            if( knownCredentials.Count > 1 )
                _logger?.LogWarning( "Found multiple credentials for projection {projType}, using the first one",
                                     projType );
        }

        while( true )
        {
            if( credentials != null )
                retVal = projection.SetCredentials( credentials );

            if( retVal )
                break;

            var eventArgs = new CredentialsNeededEventArgs( projInfo.Name );
            CredentialsNeeded?.Invoke( this, eventArgs );

            if( eventArgs.CancelImmediately || cancelOnFailure )
                break;

            cancelOnFailure = eventArgs.CancelOnFailure;
            credentials = eventArgs.Credentials;
        }

        return retVal;
    }
}
