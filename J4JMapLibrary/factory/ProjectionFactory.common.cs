﻿// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// 
// This file is part of ConsoleUtilities.
//
// ConsoleUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// ConsoleUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with ConsoleUtilities. If not, see <https://www.gnu.org/licenses/>.

namespace J4JSoftware.J4JMapLibrary;

public partial class ProjectionFactory
{
    private bool TryGetConstructorInfo( string name, out ProjectionInfo? result )
    {
        result = _sources.Values
                         .FirstOrDefault( x => x.Name.Equals( name, StringComparison.OrdinalIgnoreCase ) );

        if( result != null )
            return true;

        _logger.Error<string>( "{0} is not a known map projection name", name );
        return false;
    }

    private bool TryGetConstructorInfo( Type projType, out ProjectionInfo? result )
    {
        result = _sources.Values
                         .FirstOrDefault( x => x.MapProjectionType == projType );

        if( result != null )
            return true;

        _logger.Error( "{0} is not a known map projection type", projType );
        return false;
    }

    private object?[] CreateConstructorArguments( List<ParameterInfo> parameterSlots, params ParameterValue[] values )
    {
        if( parameterSlots.Count != values.Length )
        {
            _logger.Fatal( "Inconsistent number of parameter slots ({0}) and values ({1})",
                           parameterSlots.Count,
                           values.Length );
            return Array.Empty<object?>();
        }

        var retVal = new object?[ values.Length ];

        foreach( var slot in parameterSlots.OrderBy( x => x.Position ) )
        {
            retVal[ slot.Position ] = values.FirstOrDefault( x => x.Type == slot.Type )?.Value;
        }

        return retVal;
    }

    private bool TryCreateProjection(
        ProjectionInfo ctorInfo,
        List<ParameterValue> parameterValues,
        out IProjection? result
    )
    {
        result = null;

        if( ctorInfo.BaseConstructor == null )
        {
            _logger.Error( "{0} does not have a basic constructor", ctorInfo.MapProjectionType );
            return false;
        }

        try
        {
            var arguments = CreateConstructorArguments( ctorInfo.BaseConstructor, parameterValues.ToArray() );
            result = (IProjection?) Activator.CreateInstance( ctorInfo.MapProjectionType, arguments );
        }
        catch( Exception ex )
        {
            _logger.Error<Type, string>( "Failed to create instance of {0}, message was '{1}'",
                                         ctorInfo.MapProjectionType,
                                         ex.Message );
            return false;
        }

        if( result != null )
            return true;

        _logger.Error( "Failed to create instance of {0}", ctorInfo.MapProjectionType );
        return false;
    }

    private bool TryCreateProjectionConfigurationCredentials(
        ProjectionInfo ctorInfo,
        List<ParameterValue> parameterValues,
        out IProjection? result
    )
    {
        result = null;

        if( ctorInfo.ConfigurationCredentialedConstructor == null )
        {
            _logger.Error( "{0} does not have a constructor supporting using configured credentials",
                           ctorInfo.MapProjectionType );
            return false;
        }

        try
        {
            var arguments =
                CreateConstructorArguments( ctorInfo.ConfigurationCredentialedConstructor, parameterValues.ToArray() );

            result = (IProjection?) Activator.CreateInstance( ctorInfo.MapProjectionType, arguments );
        }
        catch( Exception ex )
        {
            _logger.Error<Type, string>( "Failed to create instance of {0}, message was '{1}'",
                                         ctorInfo.MapProjectionType,
                                         ex.Message );
            return false;
        }

        if( result != null )
            return true;

        _logger.Error( "Failed to create instance of {0}", ctorInfo.MapProjectionType );
        return false;
    }
}
