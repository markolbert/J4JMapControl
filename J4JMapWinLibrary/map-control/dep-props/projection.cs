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
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;
using J4JSoftware.WindowsUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly ThrottleDispatcher _throttleRegionChanges = new();

    private IProjection? _projection;
    private ProjectionFactory? _projFactory;

    public ProjectionFactory? ProjectionFactory
    {
        get => _projFactory;

        set
        {
            _projFactory = value;

            if( _projFactory == null )
                return;

            if( _projFactory.InitializeFactory() )
                UpdateProjection();
            else _logger?.LogError( "Projection factory failed to find projection classes" );
        }
    }

    public DependencyProperty MapProjectionProperty = DependencyProperty.Register( nameof( MapProjection ),
        typeof( string ),
        typeof( J4JMapControl ),
        new PropertyMetadata( null ) );

    public string MapProjection
    {
        get => (string)GetValue(MapProjectionProperty);

        set
        {
            SetValue( MapProjectionProperty, value );
            UpdateProjection();
        }
    }

    public DependencyProperty MapStylesProperty = DependencyProperty.Register(nameof(MapStyles),
                                                                             typeof(List<string>),
                                                                             typeof(J4JMapControl),
                                                                             new PropertyMetadata(null));

    public List<string> MapStyles
    {
        get => (List<string>) GetValue( MapStylesProperty );
        set => SetValue( MapStylesProperty, value );
    }

    private void UpdateProjection()
    {
        if( ProjectionFactory == null )
        {
            _logger?.LogError("ProjectionFactory is not defined");
            return;
        }

        var projResult = ProjectionFactory.CreateProjection( MapProjection );
        if( !projResult.ProjectionTypeFound )
        {
            _logger?.LogCritical("Could not create projection '{0}'", MapProjection);
            throw new InvalidOperationException( $"Could not create projection '{MapProjection}'" );
        }

        if( !projResult.Authenticated )
        {
            _logger?.LogCritical( $"Could not authenticate projection '{0}'", MapProjection );
            throw new InvalidOperationException( $"Could not authenticate projection '{MapProjection}'" );
        }

        _projection = projResult.Projection!;
        _projection.LoadComplete += ( _, _ ) => _dispatcherQueue.TryEnqueue( LoadMapImages );

        UpdateCaching();

        MapStyles = _projection.MapStyles.ToList();
        MapStyle = _projection.MapStyle ?? string.Empty;

        if( !Extensions.TryParseToLatLong( Center, out var latitude, out var longitude ) )
            _logger?.LogError("Could not parse Center ('{0}') to latitude/longitude, defaulting to 0/0", Center);

        if( MapRegion != null )
        {
            MapRegion.ConfigurationChanged -= MapRegionConfigurationChanged;
            MapRegion.BuildUpdated -= MapRegionBuildUpdated;
        }

        MapRegion = new MapRegion( _projection, LoggerFactory )
                   .Center( latitude, longitude )
                   .Scale( (int) MapScale )
                   .Heading( (float) Heading )
                   .Size( (float) Height, (float) Width );

        MapRegion.ConfigurationChanged += MapRegionConfigurationChanged;
        MapRegion.BuildUpdated += MapRegionBuildUpdated;

        MinMapScale = _projection.MinScale;
        MaxMapScale = _projection.MaxScale;

        MapRegion.Update();
    }
}
