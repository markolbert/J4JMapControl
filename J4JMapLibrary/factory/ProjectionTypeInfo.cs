#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ProjectionTypeInfo.cs
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

using System.Reflection;
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
