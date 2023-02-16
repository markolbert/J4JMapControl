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

using J4JSoftware.Logging;

namespace J4JSoftware.J4JMapLibrary;

public partial class ProjectionFactory
{
    private record TypeInfoBase
    {
        protected TypeInfoBase(
            Type type
        )
        {
            ProcessType( type );
        }

        protected List<List<ParameterInfo>> Constructors { get; } = new();

        private void ProcessType( Type type )
        {
            foreach( var ctor in type.GetConstructors() )
            {
                var argList = new List<ParameterInfo>();

                var ctorParams = ctor.GetParameters();

                for( var argIdx = 0; argIdx < ctorParams.Length; argIdx++ )
                {
                    var argAliases = ctorParams[ argIdx ].ParameterType.GetInterfaces().ToList();
                    argAliases.Add( ctorParams[ argIdx ].ParameterType );

                    var optional = ctorParams[ argIdx ].IsOptional;

                    if( argAliases.Any( x => x.FullName == null ) )
                    {
                        argList.Add( new ParameterInfo( argIdx, ParameterType.Other, optional ) );
                        continue;
                    }

                    if( argAliases.Any( x => x.FullName!.Equals( typeof( IJ4JLogger ).FullName ) ) )
                    {
                        argList.Add( new ParameterInfo( argIdx, ParameterType.Logger, optional ) );
                        continue;
                    }

                    if( argAliases.Any( x => x.FullName!.Equals( typeof( IProjectionCredentials ).FullName ) ) )
                    {
                        argList.Add( new ParameterInfo( argIdx, ParameterType.Credentials, optional ) );
                        continue;
                    }

                    argList.Add( argAliases.Any( x => x.FullName!.Equals( typeof( ITileCache ).FullName ) )
                                     ? new ParameterInfo( argIdx, ParameterType.TileCache, optional )
                                     : new ParameterInfo( argIdx, ParameterType.Other, optional ) );
                }

                Constructors.Add( argList );
            }
        }

        private bool ConstructorMatches( ParameterType[] requiredTypes, List<ParameterInfo> paramList )
        {
            var requiredArgs = paramList
                              .Where( x => requiredTypes.Any( y => y == x.Type ) )
                              .Distinct()
                              .ToList();

            var otherArgs = paramList
                           .Where( x => requiredTypes.All( y => y != x.Type ) )
                           .Distinct()
                           .ToList();

            if( requiredArgs.Count != requiredTypes.Length )
                return false;

            if( !otherArgs.Any() || otherArgs.All( x => x.Optional ) )
                return true;

            return false;
        }

        public List<ParameterInfo>? GetConstructor( ParameterType[] requiredTypes )
        {
            foreach( var paramList in Constructors )
            {
                if( !ConstructorMatches( requiredTypes, paramList ) )
                    continue;

                return paramList;
            }

            return null;
        }
    }
}
