#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// projection.cs
//
// This file is part of JumpForJoy Software's J4JMapWinLibrary.
// 
// J4JMapWinLibrary is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JMapWinLibrary is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JMapWinLibrary. If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;
using J4JSoftware.WindowsUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public event EventHandler<ValidCredentialsEventArgs>? ValidCredentials;

    private readonly DispatcherQueue _dispatcherLoadImages = DispatcherQueue.GetForCurrentThread();
    private readonly DispatcherQueue _dispatcherChangeProjection = DispatcherQueue.GetForCurrentThread();
    private readonly ThrottleDispatcher _throttleRegionChanges = new();
    private readonly ThrottleDispatcher _throttleProjChanges = new();

    private IProjection? _projection;

    public List<string> MapProjections { get; }

    public DependencyProperty MapProjectionProperty = DependencyProperty.Register( nameof( MapProjection ),
        typeof( string ),
        typeof( J4JMapControl ),
        new PropertyMetadata( null ) );

    public string? MapProjection
    {
        get => (string?) GetValue( MapProjectionProperty );

        set
        {
            // don't let null values overwrite non-null values
            if( GetValue( MapProjectionProperty ) != null && value == null )
            {
                _logger?.LogWarning( "Trying to overwrite non-null {prop} with null value, rejecting change",
                                     nameof( MapProjection ) );
                return;
            }

            if( !MapControlViewModelLocator.Instance!.ProjectionFactory.HasProjection( value ) )
                _logger?.LogWarning( "'{proj}' is not an available map projection", value );

            SetValue( MapProjectionProperty, value );

            // don't initialize projection if we haven't loaded yet -- the ctor
            // calls the initializer via the Loaded event
            if( !IsLoaded )
                return;

            _dispatcherChangeProjection.TryEnqueue( async () => await InitializeProjectionAsync() );
        }
    }

    private async Task InitializeProjectionAsync( CancellationToken ctx = default )
    {
        if( string.IsNullOrEmpty( MapProjection ) || !IsLoaded )
            return;

        var newProjection = MapControlViewModelLocator
                           .Instance!
                           .ProjectionFactory
                           .CreateProjection( MapProjection );

        if( newProjection == null )
        {
            _logger?.LogCritical( "Could not create projection '{proj}'", MapProjection );
            return;
        }

        if( !await AuthenticateProjection( newProjection,
                                           MapControlViewModelLocator.Instance.CredentialsFactory[ MapProjection ],
                                           ctx ) )
        {
            await ShowMessageAsync( $"Failed to authenticate {MapProjection}", "Authentication Failure" );
            return;
        }

        if( _projection != null )
        {
            _projection.LoadComplete -= ProjectionOnLoadComplete;
            _projection = null;
        }

        _projection = newProjection;
        _projection.LoadComplete += ProjectionOnLoadComplete;

        InitializeCaching();

        MapStyles = _projection.MapStyles.ToList();
        MapStyle = _projection.MapStyle ?? string.Empty;

        if( !MapExtensions.TryParseToLatLong( Center, out var latitude, out var longitude ) )
            _logger?.LogError( "Could not parse Center ('{center}') to latitude/longitude, defaulting to 0/0", Center );

        if( MapRegion != null )
        {
            MapRegion.ConfigurationChanged -= MapRegionConfigurationChanged;
            MapRegion.BuildUpdated -= MapRegionBuildUpdated;
        }

        MapRegion = new MapRegion( _projection, MapControlViewModelLocator.Instance.LoggerFactory )
                   .Center( latitude, longitude )
                   .Scale( (int) MapScale )
                   .Heading( (float) Heading )
                   .Size( (float) ActualHeight, (float) ActualWidth );

        MapRegion.ConfigurationChanged += MapRegionConfigurationChanged;
        MapRegion.BuildUpdated += MapRegionBuildUpdated;

        MinMapScale = _projection.MinScale;
        MaxMapScale = _projection.MaxScale;

        MapRegion.Update();
    }

    private async Task<bool> AuthenticateProjection(
        IProjection projection,
        ICredentials? credentials,
        CancellationToken ctx = default
    )
    {
        var credDialogType = MapControlViewModelLocator.Instance!.CredentialsDialogFactory[ MapProjection! ];

        switch( credentials )
        {
            case null when credDialogType == null:
                _logger?.LogError( "Could not find credentials dialog for {projection}", MapProjection );
                return false;

            case null:
            {
                var credDialog = await GetCredentialsFromUserAsync( credDialogType );
                credentials = credDialog?.Credentials;

                if( credentials == null )
                    return false;

                break;
            }
        }

        var cancelOnFailure = false;
        var attemptNumber = 0;

        while( true )
        {
            if( projection.SetCredentials( credentials ) && await projection.AuthenticateAsync( ctx ) )
            {
                ValidCredentials?.Invoke( this,
                                          new ValidCredentialsEventArgs( projection.Name,
                                                                         credentials,
                                                                         attemptNumber ) );
                return true;
            }

            attemptNumber++;

            if( cancelOnFailure )
                return false;

            var credDialog = await GetCredentialsFromUserAsync( credDialogType! );
            credentials = credDialog?.Credentials;

            if( credentials == null )
                return false;

            cancelOnFailure = credDialog?.CancelOnFailure ?? false;
        }
    }

    private async Task<ICredentialsDialog?> GetCredentialsFromUserAsync( Type credDialogType )
    {
        var credDialog = Activator.CreateInstance( credDialogType ) as ContentDialog;
        if( credDialog == null )
        {
            _logger?.LogError( "{dlgType} is not a {correct}", credDialogType, typeof( ContentDialog ) );
            return null;
        }

        credDialog.XamlRoot = XamlRoot;

        if( await credDialog.ShowAsync() != ContentDialogResult.Primary )
            return null;

        return (ICredentialsDialog) credDialog;
    }

    private void ProjectionOnLoadComplete( object? sender, bool e ) =>
        _dispatcherLoadImages.TryEnqueue( LoadMapImages );

    public DependencyProperty MapStylesProperty = DependencyProperty.Register( nameof( MapStyles ),
                                                                               typeof( List<string> ),
                                                                               typeof( J4JMapControl ),
                                                                               new PropertyMetadata( null ) );

    public List<string> MapStyles
    {
        get => (List<string>) GetValue( MapStylesProperty );
        set => SetValue( MapStylesProperty, value );
    }
}
