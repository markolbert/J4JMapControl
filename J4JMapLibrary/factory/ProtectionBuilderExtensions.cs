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

using J4JSoftware.J4JMapLibrary.MapBuilder;

namespace J4JSoftware.J4JMapLibrary;

public static class ProtectionBuilderExtensions
{
    public static ProjectionBuilder EnableCaching(
        this ProjectionBuilder builder,
        ITileCache cache
    )
    {
        builder.Cache = cache;
        return builder;
    }

    public static ProjectionBuilder DisableCaching(
        this ProjectionBuilder builder
    )
    {
        builder.Cache = null;
        return builder;
    }

    public static ProjectionBuilder Credentials(
        this ProjectionBuilder builder,
        object? credentials
    )
    {
        builder.Credentials = credentials;
        return builder;
    }

    public static ProjectionBuilder Authenticate(
        this ProjectionBuilder builder
    )
    {
        builder.Authenticate = true;
        return builder;
    }

    public static ProjectionBuilder SkipAuthentication(
        this ProjectionBuilder builder
    )
    {
        builder.Authenticate = false;
        return builder;
    }

    public static ProjectionBuilder Projection(
        this ProjectionBuilder builder,
        string projectionName
    )
    {
        builder.ProjectionName = projectionName;
        builder.ProjectionType = null;
        return builder;
    }

    public static ProjectionBuilder Projection(
        this ProjectionBuilder builder,
        Type projectionType
    )
    {
        builder.ProjectionName = null;
        builder.ProjectionType = projectionType;
        return builder;
    }

    public static ProjectionBuilder Projection<TProj>(
        this ProjectionBuilder builder
    )
        where TProj : IProjection =>
        builder.Projection( typeof( TProj ) );

    public static ProjectionBuilder RequestLatency(
        this ProjectionBuilder builder,
        int maxRequestLatency
    )
    {
        builder.MaxRequestLatency = maxRequestLatency;
        return builder;
    }
}
