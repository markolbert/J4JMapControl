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
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using J4JSoftware.WindowsUtilities;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    private readonly ThrottleDispatcher _throttlePoiSourceChange = new();
    private readonly ThrottleDispatcher _throttlePoiItemChange = new();

    private object? _pointsOfInterestSource;
    private DataSourceValidator<J4JMapControl>? _poiSourceValidator;
    private string? _pointOfInterestLocationProp;
    private List<GeoData>? _pointsOfInterest;

    public static readonly DependencyProperty PointsOfInterestProperty = DependencyProperty.Register(nameof(PointsOfInterest),
                                                                                                     typeof(List<FrameworkElement>),
                                                                                                     typeof(J4JMapControl),
                                                                                                     new PropertyMetadata(new List<FrameworkElement>()));

    public List<FrameworkElement> PointsOfInterest
    {
        get => (List<FrameworkElement>)GetValue(PointsOfInterestProperty);
        set => SetValue(PointsOfInterestProperty, value);
    }

    public object? PointsOfInterestSource
    {
        get => _pointsOfInterestSource;

        set
        {
            if ( _pointsOfInterestSource is System.Collections.Specialized.INotifyCollectionChanged temp )
                temp.CollectionChanged -= PoiCollectionChanged;

            _pointsOfInterestSource = value;

            if (_pointsOfInterestSource is System.Collections.Specialized.INotifyCollectionChanged temp2)
                temp2.CollectionChanged += PoiCollectionChanged;

            InitializePointsOfInterest();
        }
    }

    private void PoiCollectionChanged( object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
    {
        _throttlePoiSourceChange.Throttle(UpdateEventInterval, _ =>
        {
            InitializePointsOfInterest();
            IncludeAnnotations();
        });
    }

    private void InitializePointsOfInterest()
    {
        if( _poiSourceValidator == null )
            return;

        switch (_poiSourceValidator.Validate(PointsOfInterestSource, out var processed))
        {
            case DataSourceValidationResult.SourceNotEnumerable:
                _logger?.LogWarning("Data source {source} is not enumerable", nameof(PointsOfInterestSource));
                return;

            case DataSourceValidationResult.UndefinedSource:
                _logger?.LogWarning("Data source {source} is not defined", nameof(PointsOfInterestSource));
                return;

            case DataSourceValidationResult.Unprocessed:
                _logger?.LogWarning("Data source {source} was not validated", nameof(PointsOfInterestSource));
                return;

            case DataSourceValidationResult.Processed:
                // no op; proceed, but warn of oddities
                if( processed.Any(
                       x => x.ValidationResults.Any( y => y.Value != DataItemValidationResult.Validated ) ) )
                    _logger?.LogWarning( "Data source {source} was validated but errors were found",
                                         nameof( PointsOfInterestSource ) );

                break;
        }

        foreach ( var item in _pointsOfInterest ?? Enumerable.Empty<object>() )
        {
            if( item is INotifyPropertyChanged propChanged )
                propChanged.PropertyChanged -= PoiItemPropertyChanged;
        }

        var validItems = processed
                        .Where( x => x.ValidationResults.All( y => y.Value == DataItemValidationResult.Validated ) )
                        .Select( x => x.Item )
                        .ToList();

        foreach ( var item in validItems )
        {
            if (item is not INotifyPropertyChanged propChanged)
                continue;

            propChanged.PropertyChanged += PoiItemPropertyChanged;
        }

        var factory = new GeoDataFactory( PointsOfInterestLocationProperty!, _loggerFactory );
        _pointsOfInterest = factory.Create( validItems ).ToList();
    }

    private void PoiItemPropertyChanged( object? sender, PropertyChangedEventArgs e ) =>
        _throttlePoiItemChange.Throttle( UpdateEventInterval, _ =>
        {
            IncludeAnnotations();
        });

    public string? PointsOfInterestLocationProperty
    {
        get => _pointOfInterestLocationProp;

        set
        {
            _pointOfInterestLocationProp = value;
            InitializePointsOfInterest();
        }
    }

    public DataTemplate? PointsOfInterestTemplate { get; set; }
}
