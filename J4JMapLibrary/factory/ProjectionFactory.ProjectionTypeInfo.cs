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

using System.Reflection;

namespace J4JMapLibrary;

public partial class ProjectionFactory
{
    private record ProjectionTypeInfo : TypeInfoBase
    {
        private static readonly ParameterType[] TiledParameters = { ParameterType.Logger, ParameterType.TileCache };

        private static readonly ParameterType[] TiledCredentialedParameters =
        {
            ParameterType.Logger, ParameterType.TileCache, ParameterType.Credentials
        };

        private static readonly ParameterType[] StaticParameters = { ParameterType.Logger };

        private static readonly ParameterType[] StaticCredentialedParameters =
        {
            ParameterType.Logger, ParameterType.Credentials
        };

        public ProjectionTypeInfo(
            Type projType
        )
            : base( projType )
        {
            ProjectionType = projType;

            var attr = projType.GetCustomAttribute<ProjectionAttribute>();
            Name = attr?.Name ?? string.Empty;

            if( projType.IsAssignableTo( typeof( ITiledProjection ) ) )
            {
                IsTiled = true;
                BasicConstructor = GetConstructor( TiledParameters );
                ConfigurationCredentialConstructor = GetConstructor( TiledCredentialedParameters );
            }
            else
            {
                BasicConstructor = GetConstructor( StaticParameters );
                ConfigurationCredentialConstructor = GetConstructor( StaticCredentialedParameters );
            }
        }

        public string Name { get; }
        public Type ProjectionType { get; }
        public bool IsTiled { get; }

        public List<ParameterInfo>? BasicConstructor { get; }
        public List<ParameterInfo>? ConfigurationCredentialConstructor { get; }
    }
}
