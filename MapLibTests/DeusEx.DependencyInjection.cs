#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// DeusEx.DependencyInjection.cs
//
// This file is part of JumpForJoy Software's MapLibTests.
// 
// MapLibTests is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// MapLibTests is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with MapLibTests. If not, see <https://www.gnu.org/licenses/>.
#endregion

using Autofac;
using J4JSoftware.J4JMapLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MapLibTests;

internal partial class DeusEx
{
    private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
    {
        builder.Register( c =>
                {
                    var retVal = new ProjectionFactory( c.Resolve<ILoggerFactory>() );

                    retVal.InitializeFactory();

                    return retVal;
                } )
               .AsSelf()
               .SingleInstance();

        builder.Register(c =>
                {
                    var retVal = new CredentialsFactory(c.Resolve<IConfiguration>(), c.Resolve<ILoggerFactory>());

                    retVal.InitializeFactory();

                    return retVal;
                })
               .AsSelf()
               .SingleInstance();

        builder.Register( c => new MemoryCache( "In Memory", c.Resolve<ILoggerFactory>() ) )
               .AsSelf();

        builder.Register( c => new FileSystemCache( "File System", c.Resolve<ILoggerFactory>() ) )
               .AsSelf();
    }
}
