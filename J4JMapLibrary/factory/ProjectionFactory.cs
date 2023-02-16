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

using System.Collections.ObjectModel;
using System.Reflection;
using J4JSoftware.Logging;

namespace J4JSoftware.J4JMapLibrary;

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
                         .Where( x => x.GetInterface( nameof( IProjection ) ) != null )
                         .Select( x => new ProjectionTypeInfo( x ) )
                         .Where( x => x.BasicConstructor != null
                                  && !string.IsNullOrEmpty( x.Name ) )
                         .ToList();

        foreach( var projInfo in projections )
        {
            if( projInfo.BasicConstructor == null && projInfo.ConfigurationCredentialConstructor == null )
                continue;

            _sources.Add( projInfo.Name,
                          new ProjectionInfo( projInfo.Name,
                                              projInfo.ProjectionType,
                                              projInfo.IsTiled,
                                              projInfo.BasicConstructor,
                                              projInfo.ConfigurationCredentialConstructor ) );
        }
    }

    private record ProjectionInfo(
        string Name,
        Type MapProjectionType,
        bool IsTiled,
        List<ParameterInfo>? BaseConstructor,
        List<ParameterInfo>? ConfigurationCredentialedConstructor
    );

    private enum ParameterType
    {
        TileCache,
        Credentials,
        Logger,
        Other
    }

    private record ParameterValue( ParameterType Type, object? Value );

    private record ParameterInfo( int Position, ParameterType Type, bool Optional );
}
