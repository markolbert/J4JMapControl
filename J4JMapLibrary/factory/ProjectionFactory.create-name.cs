// Copyright (c) 2021, 2022 Mark A. Olbert 
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
    public async Task<ProjectionCreationResult> CreateMapProjection(
        string projectionName,
        object credentials,
        ITileCache? tileCache,
        IMapServer? mapServer = null,
        bool authenticate = true,
        CancellationToken ctx = default
    )
    {
        if( !TryGetConstructorInfo( projectionName, out var ctorInfo ) )
            return ProjectionCreationResult.NoProjection;

        var ctorArgs = new List<ParameterValue> { new( ParameterType.Logger, _logger ) };

        if( ctorInfo!.IsTiled )
            ctorArgs.Add( new ParameterValue( ParameterType.TileCache, tileCache ) );

        if( !TryCreateProjection( ctorInfo, ctorArgs, out var mapProjection ) )
            return ProjectionCreationResult.NoProjection;

        if( await mapProjection!.AuthenticateAsync( credentials, ctx ) )
            return new ProjectionCreationResult( mapProjection, true );

        _logger.Warning( "Supplied credentials failed, attempting to use configured credentials" );
        return await CreateMapProjection( projectionName, tileCache, mapServer, authenticate, ctx );
    }

    public async Task<ProjectionCreationResult> CreateMapProjection(
        string projectionName,
        ITileCache? tileCache,
        IMapServer? mapServer = null,
        bool authenticate = true,
        CancellationToken ctx = default
    )
    {
        if( !TryGetConstructorInfo( projectionName, out var ctorInfo ) )
            return ProjectionCreationResult.NoProjection;

        //if( !EnsureMapServer( ctorInfo!, ref mapServer ) )
        //    return ProjectionCreationResult.NoProjection;

        var ctorArgs = new List<ParameterValue>
        {
            //new( ParameterType.MapServer, mapServer ),
            new( ParameterType.Logger, _logger ), new( ParameterType.Credentials, ProjectionCredentials )
        };

        if( ctorInfo!.IsTiled )
            ctorArgs.Add( new ParameterValue( ParameterType.TileCache, tileCache ) );

        if( !TryCreateProjectionConfigurationCredentials( ctorInfo!, ctorArgs, out var mapProjection ) )
            return ProjectionCreationResult.NoProjection;

        if( !authenticate || await mapProjection!.AuthenticateAsync( null, ctx ) )
            return new ProjectionCreationResult( mapProjection, authenticate );

        _logger.Error( "Authentication of {0} instance failed", ctorInfo!.MapProjectionType );
        return new ProjectionCreationResult( mapProjection, false );
    }
}
