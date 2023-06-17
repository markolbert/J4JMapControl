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
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.J4JMapLibrary;
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

        _projection = newProjection;

        InitializeCaching();

        MapStyles = _projection.MapStyles.ToList();
        MapStyle = _projection.MapStyle ?? string.Empty;

        if( !MapExtensions.TryParseToLatLong( Center, out var latitude, out var longitude ) )
            _logger?.LogError( "Could not parse CenterPoint ('{center}') to latitude/longitude, defaulting to 0/0", Center );

        if( RegionView != null )
        {
            //RegionView.ImagesLoaded -= OnImagesLoaded;
            RegionView.PropertyChanged -= RegionViewOnPropertyChanged;
        }

        RegionView = _projection switch
        {
            ITiledProjection tiledProj => new TiledRegionView( tiledProj ),
            StaticProjection staticProj => new StaticRegionView( staticProj ),
            _ => null
        };

        if( RegionView == null )
        {
            _logger?.LogError( "Unsupported {projType} '{type}'", typeof( IProjection ), _projection.GetType() );
            return;
        }

        //RegionView.ImagesLoaded += OnImagesLoaded;
        RegionView.PropertyChanged += RegionViewOnPropertyChanged;

        MinMapScale = _projection.MinScale;
        MaxMapScale = _projection.MaxScale;
    }

    private void RegionViewOnPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        switch( e.PropertyName )
        {
            case "Latitude":
            case "Longitude":
                if( RegionView?.Center != null )
                {
                    SetValue( CenterProperty,
                           MapExtensions.ConvertToLatLongText( RegionView.Center.Latitude,
                                                               RegionView.Center.Longitude ) );

                    _centerLatitude = RegionView.Center.Latitude;
                    _centerLongitude = RegionView.Center.Longitude;
                }

                break;

            case "Scale":
                if( RegionView != null )
                    SetValue( MapScaleProperty, RegionView.AdjustedRegion.Scale );

                break;
        }
    }

    //private void OnImagesLoaded(object? sender, bool loaded)
    //{
    //    if (loaded)
    //        _dispatcherLoadImages.TryEnqueue(UpdateDisplay);
    //    else _logger?.LogError("Failed to load region view images");
    //}

    private async Task<bool> AuthenticateProjection(
        IProjection projection,
        ICredentials? credentials,
        CancellationToken ctx = default
    )
    {
        var credDialogType = MapControlViewModelLocator.Instance!.CredentialsDialogFactory[ MapProjection! ];
        var attemptNumber = 0;

        switch ( credentials )
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

                attemptNumber++;

                break;
            }
        }

        var cancelOnFailure = false;

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
