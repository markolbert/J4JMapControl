// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
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
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;

namespace J4JSoftware.J4JMapLibrary;

public abstract class Projection<TAuth, TViewport, TFrag> : IProjection
    where TAuth : class
    where TFrag : IMapFragment
    where TViewport : INormalizedViewport
{
    protected Projection(
        IJ4JLogger logger
    )
    {
        Logger = logger;
        Logger.SetLoggedType( GetType() );

        var attribute = GetType().GetCustomAttribute<ProjectionAttribute>();
        if( attribute == null )
            Logger.Error( "Map projection class is not decorated with ProjectionAttribute(s), cannot be used" );
        else Name = attribute.Name;
    }

    protected Projection(
        IProjectionCredentials credentials,
        IJ4JLogger logger
    )
    {
        Logger = logger;
        Logger.SetLoggedType( GetType() );

        var attributes = GetType().GetCustomAttributes<ProjectionAttribute>().ToList();
        if( !attributes.Any() )
        {
            Logger.Fatal( "Map projection class is not decorated with ProjectionAttribute(s)" );
            throw new ApplicationException( "Map projection class is not decorated with ProjectionAttribute(s)" );
        }

        Name = attributes.First().Name;

        LibraryConfiguration = credentials;

        var attribute = GetType().GetCustomAttribute<ProjectionAttribute>();
        if( attribute == null )
            Logger.Error( "Map projection class is not decorated with ProjectionAttribute(s), cannot be used" );
        else Name = attribute.Name;
    }

    protected IJ4JLogger Logger { get; }
    protected IProjectionCredentials? LibraryConfiguration { get; }

    public string Name { get; } = string.Empty;

    public abstract IProjectionScale MapScale { get; }
    public abstract IMapServer MapServer { get; }

    public virtual bool Initialized => !string.IsNullOrEmpty( Name ) && MapServer.Initialized;

    public abstract Task<bool> AuthenticateAsync( TAuth? credentials, CancellationToken ctx = default );

    public abstract IAsyncEnumerable<TFrag> GetExtractAsync(
        TViewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );

    async Task<bool> IProjection.AuthenticateAsync( object? credentials, CancellationToken ctx )
    {
        switch( credentials )
        {
            case TAuth castCredentials:
                return await AuthenticateAsync( castCredentials, ctx );

            case null:
                return await AuthenticateAsync( null, ctx );

            default:
                Logger.Error( "Expected a {0} but received a {1}", typeof( TAuth ), credentials.GetType() );
                return false;
        }
    }

    async IAsyncEnumerable<IMapFragment> IProjection.GetExtractAsync(
        INormalizedViewport viewportData,
        bool deferImageLoad,
        [ EnumeratorCancellation ] CancellationToken ctx
    )
    {
        if( viewportData is TViewport castData )
        {
            await foreach( var fragment in GetExtractAsync( castData, deferImageLoad, ctx ) )
            {
                yield return fragment;
            }
        }

        Logger.Error( "Expected viewport data to be an {0}, got a {1} instead",
                      typeof( TViewport ),
                      viewportData.GetType() );
    }
}
