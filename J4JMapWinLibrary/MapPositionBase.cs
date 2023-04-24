using System;
using J4JSoftware.WindowsUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.ComponentModel;

namespace J4JSoftware.J4JMapWinLibrary;

public class MapPositionBase : DependencyObject
{
    public event EventHandler? SourceUpdated;

    private readonly ThrottleDispatcher _throttleSourceChange = new();
    private readonly ThrottleDispatcher _throttleItemChange = new();
    private readonly IDataSourceValidator _srcValidator;
    private readonly int _updateInterval;
    private readonly string _srcName;
    private readonly ILogger? _logger;

    private object? _source;
    private string? _latProp;
    private string? _longProp;
    private string? _latLongProp;

    protected MapPositionBase(
        IDataSourceValidator srcValidator,
        string srcName,
        ILoggerFactory? loggerFactory = null,
        int updateInterval = J4JMapControl.DefaultUpdateEventInterval
    )
    {
        _srcValidator = srcValidator;
        _srcName = srcName;
        _updateInterval = updateInterval < 0 ? J4JMapControl.DefaultUpdateEventInterval : updateInterval;
        _logger = loggerFactory?.CreateLogger<MapPositionBase>();
    }

    public string? LatitudeProperty
    {
        get => _latProp;
        set => _latProp = value;
    }

    public string? LongitudeProperty
    {
        get => _longProp;
        set => _longProp = value;
    }

    public string? LatLongProperty
    {
        get => _latLongProp;
        set => _latLongProp = value;
    }

    public object? Source
    {
        get => _source;

        set
        {
            if (_source is System.Collections.Specialized.INotifyCollectionChanged temp)
                temp.CollectionChanged -= SourceCollectionChanged;

            _source = value;

            if (_source is System.Collections.Specialized.INotifyCollectionChanged temp2)
                temp2.CollectionChanged += SourceCollectionChanged;

            InitializeSource();
            SourceUpdated?.Invoke(this, EventArgs.Empty  );
        }
    }

    public List<object> ValidItems { get; private set; } = new();

    private void SourceCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        _throttleSourceChange.Throttle(_updateInterval, _ =>
        {
            InitializeSource();
            SourceUpdated?.Invoke(this, EventArgs.Empty);
        });
    }

    private void InitializeSource()
    {
        switch( _srcValidator.Validate( Source, _srcName, out var validItems ) )
        {
            case DataSourceValidationResult.UndefinedPropertyName:
                _logger?.LogTrace( "Properties not defined when validating {source} data source",
                                   _srcName );
                return;

            case DataSourceValidationResult.Success:
                // no op; proceed
                break;

            default:
                _logger?.LogError( "Errors encountered when validating {source} data source",
                                   _srcName );
                return;
        }

        // decouple property changed event handler from existing items
        foreach (var item in ValidItems )
        {
            if (item is INotifyPropertyChanged propChanged)
                propChanged.PropertyChanged -= ItemPropertyChanged;
        }

        // set up property changed event handler for valid items
        foreach (var item in validItems)
        {
            if (item is not INotifyPropertyChanged propChanged)
                continue;

            propChanged.PropertyChanged += ItemPropertyChanged;
        }

        ValidItems = validItems;
    }

    private void ItemPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        _throttleItemChange.Throttle(_updateInterval, _ =>
        {
            SourceUpdated?.Invoke( this, EventArgs.Empty );
        });
}
