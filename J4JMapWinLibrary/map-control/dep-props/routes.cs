#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// annotations.cs
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
using System.ComponentModel;
using System.Linq;
using Microsoft.UI.Xaml;
using J4JSoftware.WindowsUtilities;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private readonly ThrottleDispatcher _throttleRoutesSourceChange = new();
    private readonly ThrottleDispatcher _throttleRoutesItemChange = new();

    private Canvas? _routesCanvas;

    private object? _routesSource;
    private string? _routesLocationProp;
    private DataSourceValidator<J4JMapControl>? _routeSourceValidator;
    private List<GeoData>? _routePoints;

    public object? RoutesSource
    {
        get => _routesSource;

        set
        {
            if ( _routesSource is System.Collections.Specialized.INotifyCollectionChanged temp )
                temp.CollectionChanged -= RoutesCollectionChanged;

            _routesSource = value;

            if (_routesSource is System.Collections.Specialized.INotifyCollectionChanged temp2)
                temp2.CollectionChanged += RoutesCollectionChanged;

            InitializeRoutes();
        }
    }

    private void RoutesCollectionChanged( object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
    {
        _throttleRoutesSourceChange.Throttle(UpdateEventInterval, _ =>
        {
            InitializeRoutes();
        });
    }

    public string? RoutesLocationProperty
    {
        get => _routesLocationProp;

        set
        {
            _routesLocationProp = value;
            InitializeRoutes();
        }
    }

    private void InitializeRoutes()
    {
        if (_routeSourceValidator == null)
            return;

        switch (_routeSourceValidator.Validate(PointsOfInterestSource, out var processed))
        {
            case DataSourceValidationResult.SourceNotEnumerable:
                _logger?.LogWarning("Data source {source} is not enumerable", nameof(RoutesSource));
                return;

            case DataSourceValidationResult.UndefinedSource:
                _logger?.LogWarning("Data source {source} is not defined", nameof(RoutesSource));
                return;

            case DataSourceValidationResult.Unprocessed:
                _logger?.LogWarning("Data source {source} was not validated", nameof(RoutesSource));
                return;

            case DataSourceValidationResult.Processed:
                // no op; proceed, but warn of oddities
                if (processed.Any(
                        x => x.ValidationResults.Any(y => y.Value != DataItemValidationResult.Validated)))
                    _logger?.LogWarning("Data source {source} was validated but errors were found",
                                        nameof(RoutesSource));

                break;
        }

        foreach ( var item in _routePoints ?? Enumerable.Empty<object>() )
        {
            if( item is INotifyPropertyChanged propChanged )
                propChanged.PropertyChanged -= RouteItemPropertyChanged;
        }

        var validItems = processed
                        .Where(x => x.ValidationResults.All(y => y.Value == DataItemValidationResult.Validated))
                        .Select(x => x.Item)
                        .ToList();

        foreach ( var item in validItems )
        {
            if (item is not INotifyPropertyChanged propChanged)
                continue;

            propChanged.PropertyChanged += RouteItemPropertyChanged;
        }

        var factory = new GeoDataFactory(RoutesLocationProperty!, _loggerFactory);
        _routePoints = factory.Create(validItems).ToList();
    }

    public static readonly DependencyProperty RouteMarkersProperty = DependencyProperty.Register(nameof(RouteMarkers),
                                                                                                 typeof(List<Shape>),
                                                                                                 typeof(J4JMapControl),
                                                                                                 new PropertyMetadata(new List<FrameworkElement>()));

    public List<Shape> RouteMarkers
    {
        get => (List<Shape>)GetValue(RouteMarkersProperty);
        set => SetValue(RouteMarkersProperty, value);
    }

    private void RouteItemPropertyChanged( object? sender, PropertyChangedEventArgs e ) =>
        _throttleRoutesItemChange.Throttle( UpdateEventInterval, _ =>
        {
            IncludeRoutes();
        });
}
