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

using System.Collections.Generic;
using System.Linq;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;
using J4JSoftware.WindowsUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private readonly DispatcherQueue _dispatcherLoadImages = DispatcherQueue.GetForCurrentThread();
    private readonly ThrottleDispatcher _throttleRegionChanges = new();

    private IProjection? _projection;

    public DependencyProperty MapProjectionsProperty = DependencyProperty.Register(
        nameof( MapProjections ),
        typeof( List<string> ),
        typeof( J4JMapControl ),
        new PropertyMetadata( new List<string>() ) );

    public List<string> MapProjections
    {
        get => (List<string>) GetValue( MapProjectionsProperty );
        set => SetValue( MapProjectionsProperty, value );
    }

    public DependencyProperty MapProjectionProperty = DependencyProperty.Register( nameof( MapProjection ),
                                                                               typeof( string ),
                                                                               typeof( J4JMapControl ),
                                                                               new PropertyMetadata( null ) );

    public string? MapProjection
    {
        get => (string?)GetValue(MapProjectionProperty);

        set
        {
            if( !MapControlViewModelLocator.Instance!.ProjectionFactory.HasProjection( value ) )
                _logger?.LogWarning( "'{proj}' is not an available map projection", value );

            SetValue( MapProjectionProperty, value );
            InitializeProjection();
        }
    }

    private void InitializeProjection()
    {
        if( string.IsNullOrEmpty( MapProjection ) || !IsLoaded )
            return;

        var newProjection = MapControlViewModelLocator
                           .Instance!
                           .ProjectionFactory
                           .CreateProjection( MapProjection );

        if (newProjection == null )
        {
            _logger?.LogCritical("Could not create projection '{proj}'", MapProjection);
            return;
        }

        var credentials = MapControlViewModelLocator.Instance.CredentialsFactory[ MapProjection, true ];
        if( credentials == null )
        {
            _logger?.LogCritical("Could not find credentials for projection '{proj}'", MapProjection);
            return;
        }

        if( !newProjection.SetCredentials(credentials))
            return;

        if( !newProjection.Authenticate() )
        {
            _logger?.LogError( "Projection {projName} could not be authenticated", MapProjection );
            return;
        }

        if (_projection != null)
        {
            _projection.LoadComplete -= ProjectionOnLoadComplete;
            _projection = null;
        }

        _projection = newProjection;
        _projection.LoadComplete += ProjectionOnLoadComplete;

        InitializeCaching();

        MapStyles = _projection.MapStyles.ToList();
        MapStyle = _projection.MapStyle ?? string.Empty;

        if (!MapExtensions.TryParseToLatLong(Center, out var latitude, out var longitude))
            _logger?.LogError("Could not parse Center ('{center}') to latitude/longitude, defaulting to 0/0", Center);

        var height = (float)Height;
        var width = (float)Width;

        if (MapRegion != null)
        {
            MapRegion.ConfigurationChanged -= MapRegionConfigurationChanged;
            MapRegion.BuildUpdated -= MapRegionBuildUpdated;

            height = MapRegion.RequestedHeight;
            width = MapRegion.RequestedWidth;
        }

        MapRegion = new MapRegion(_projection, MapControlViewModelLocator.Instance.LoggerFactory)
                   .Center(latitude, longitude)
                   .Scale((int)MapScale)
                   .Heading((float)Heading)
                   .Size(height, width);

        MapRegion.ConfigurationChanged += MapRegionConfigurationChanged;
        MapRegion.BuildUpdated += MapRegionBuildUpdated;

        MinMapScale = _projection.MinScale;
        MaxMapScale = _projection.MaxScale;

        MapRegion.Update();
    }

    private void ProjectionOnLoadComplete( object? sender, bool e ) => _dispatcherLoadImages.TryEnqueue(LoadMapImages);

    public DependencyProperty MapStylesProperty = DependencyProperty.Register(nameof(MapStyles),
                                                                              typeof(List<string>),
                                                                              typeof(J4JMapControl),
                                                                              new PropertyMetadata(null));

    public List<string> MapStyles
    {
        get => (List<string>) GetValue( MapStylesProperty );
        set => SetValue( MapStylesProperty, value );
    }
}
