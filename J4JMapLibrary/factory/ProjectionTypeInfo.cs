﻿using System.Reflection;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;

namespace J4JSoftware.J4JMapLibrary;

internal record ProjectionCtorInfo( bool SupportsCaching, List<ProjectionCtorParameterType> ParameterTypes );

internal record ProjectionTypeInfo
{
    private readonly IJ4JLogger? _logger;

    public ProjectionTypeInfo(
        Type projType
        )
    {
        _logger = J4JDeusEx.GetLogger();
        _logger?.SetLoggedType( GetType() );

        var attr = projType.GetCustomAttribute<ProjectionAttribute>( false )
         ?? throw new NullReferenceException( $"{projType} is not decorated with a {typeof( ProjectionAttribute )}" );
        
        Name = attr.ProjectionName;
        ProjectionType = projType;
        TiledProjection = projType.IsAssignableTo( typeof( ITiledProjection ) );

        InitializeCtorParameters();

        if( !ConstructorInfo.Any() )
            _logger?.Error( "No supported constructors found for {0}", projType );
    }

    public string Name { get; init; }
    public Type ProjectionType { get; init; }
    public bool TiledProjection { get; init; }
    public List<ProjectionCtorInfo> ConstructorInfo { get; } = new();

    private void InitializeCtorParameters()
    {
        foreach (var ctor in ProjectionType.GetConstructors() )
        {
            // see if the ctor also accepts an ITiledCache parameter
            var supportsCaching =
                ctor.GetParameters().Any(p => p.ParameterType.IsAssignableTo(typeof(ITileCache)));

            var requiredCount = supportsCaching ? 3 : 2;

            var ctorParameters = new List<ProjectionCtorParameterType>();

            foreach (var ctorParameter in ctor.GetParameters())
            {
                if (ctorParameter.ParameterType.IsAssignableTo(typeof(IJ4JLogger)))
                {
                    ctorParameters.Add(ProjectionCtorParameterType.Logger);
                    continue;
                }

                if (ctorParameter.ParameterType.GetInterface(typeof(IMapServer).FullName ?? string.Empty) != null)
                {
                    ctorParameters.Add(ProjectionCtorParameterType.MapServer);
                    continue;
                }

                if (ctorParameter.ParameterType.IsAssignableTo(typeof(ITileCache)))
                {
                    ctorParameters.Add(ProjectionCtorParameterType.Cache);
                    continue;
                }

                ctorParameters.Add(ProjectionCtorParameterType.Other);
            }

            if( ctorParameters.Count == requiredCount )
                ConstructorInfo.Add( new ProjectionCtorInfo( supportsCaching, ctorParameters ) );
            else _logger?.Warning( "Found unsupported public constructor taking {0} parameters", ctorParameters.Count );
        }
    }
}