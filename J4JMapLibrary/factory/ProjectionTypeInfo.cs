﻿using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapLibrary;

internal record ProjectionCtorInfo( bool SupportsCaching, List<ProjectionCtorParameterType> ParameterTypes );

internal record ProjectionTypeInfo
{
    private readonly ILogger? _logger;

    public ProjectionTypeInfo(
        Type projType,
        ILoggerFactory? loggerFactory = null
    )
    {
        _logger = loggerFactory?.CreateLogger<ProjectionTypeInfo>();

        var attr = projType.GetCustomAttribute<ProjectionAttribute>( false )
         ?? throw new NullReferenceException( $"{projType} is not decorated with a {typeof( ProjectionAttribute )}" );

        Name = attr.ProjectionName;
        ProjectionType = projType;
        TiledProjection = projType.IsAssignableTo( typeof( ITiledProjection ) );

        InitializeCtorParameters();

        if( !ConstructorInfo.Any() )
            _logger?.LogError( "No supported constructors found for {0}", projType );
    }

    public string Name { get; init; }
    public Type ProjectionType { get; init; }
    public bool TiledProjection { get; init; }
    public List<ProjectionCtorInfo> ConstructorInfo { get; } = new();

    private void InitializeCtorParameters()
    {
        foreach( var ctor in ProjectionType.GetConstructors() )
        {
            // see if the ctor also accepts an ITiledCache parameter
            var supportsCaching =
                ctor.GetParameters().Any( p => p.ParameterType.IsAssignableTo( typeof( ITileCache ) ) );

            var requiredCount = supportsCaching ? 2 : 1;

            var ctorParameters = new List<ProjectionCtorParameterType>();

            foreach( var ctorParameter in ctor.GetParameters() )
            {
                if( ctorParameter.ParameterType.IsAssignableTo( typeof( ILoggerFactory ) ) )
                {
                    ctorParameters.Add( ProjectionCtorParameterType.LoggerFactory );
                    continue;
                }

                ctorParameters.Add( ProjectionCtorParameterType.Other );
            }

            if( ctorParameters.Count == requiredCount )
                ConstructorInfo.Add( new ProjectionCtorInfo( supportsCaching, ctorParameters ) );
            else _logger?.LogWarning( "Found unsupported public constructor taking {0} parameters", ctorParameters.Count );
        }
    }
}
