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

using System.Reflection;
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;
#pragma warning disable CS8618

namespace J4JSoftware.J4JMapLibrary;

public abstract class Projection<TAuth, TViewport, TFrag> : IProjection
    where TAuth : class, new()
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
        else Name = attribute.ProjectionName;
    }

    protected IJ4JLogger Logger { get; }

    public string Name { get; } = string.Empty;
    public IMapServer MapServer { get; init; }
    public virtual bool Initialized => !string.IsNullOrEmpty( Name ) && MapServer.Initialized;

    public bool Authenticate( TAuth credentials ) =>
        Task.Run( async () => await AuthenticateAsync( credentials ) ).Result;

    public abstract Task<bool> AuthenticateAsync( TAuth credentials, CancellationToken ctx = default );

    public abstract IAsyncEnumerable<TFrag> GetExtractAsync(
        TViewport viewportData,
        bool deferImageLoad = false,
        CancellationToken ctx = default
    );

    async Task<bool> IProjection.AuthenticateAsync( object credentials, CancellationToken ctx )
    {
        switch( credentials )
        {
            case TAuth castCredentials:
                return await AuthenticateAsync( castCredentials, ctx );

            default:
                Logger.Error( "Expected a {0} but received a {1}", typeof( TAuth ), credentials.GetType() );
                return false;
        }
    }

    bool IProjection.Authenticate(object credentials)
    {
        switch (credentials)
        {
            case TAuth castCredentials:
                return Authenticate(castCredentials);

            default:
                Logger.Error("Expected a {0} but received a {1}", typeof(TAuth), credentials.GetType());
                return false;
        }
    }

    async IAsyncEnumerable<IMapFragment> IProjection.GetViewportAsync(
        INormalizedViewport viewportData,
        bool deferImageLoad,
        [ EnumeratorCancellation ] CancellationToken ctx
    )
    {
        if( viewportData.GetType().IsAssignableTo( typeof( TViewport ) ) )
        {
            await foreach( var fragment in GetExtractAsync( (TViewport) viewportData, deferImageLoad, ctx ) )
            {
                yield return fragment;
            }
        }
        else
            Logger.Error( "Expected viewport data to be an {0}, got a {1} instead",
                          typeof( TViewport ),
                          viewportData.GetType() );
    }
}
