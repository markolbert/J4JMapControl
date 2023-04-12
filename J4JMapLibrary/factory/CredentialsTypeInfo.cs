#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// CredentialsTypeInfo.cs
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

namespace J4JSoftware.J4JMapLibrary;

internal record CredentialsTypeInfo
{
    public CredentialsTypeInfo(
        Type credentialsType
    )
    {
        var attr = credentialsType.GetCustomAttribute<MapCredentialsAttribute>( false )
         ?? throw new NullReferenceException(
                $"{credentialsType} is not decorated with a {typeof( MapCredentialsAttribute )}" );

        Name = attr.CredentialsName;
        ProjectionType = attr.ProjectionType;
        CredentialsType = credentialsType;
    }

    public string Name { get; init; }
    public Type CredentialsType { get; init; }
    public Type ProjectionType { get; init; }
}
