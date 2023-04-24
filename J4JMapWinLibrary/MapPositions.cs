using System;
using J4JSoftware.WindowsUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using J4JSoftware.J4JMapLibrary;

namespace J4JSoftware.J4JMapWinLibrary;

public class MapPositions : DependencyObject
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

    public MapPositions(
        IDataSourceValidator srcValidator,
        string srcName,
        ILoggerFactory? loggerFactory = null,
        int updateInterval = J4JMapControl.DefaultUpdateEventInterval
    )
    {
        _srcValidator = srcValidator;
        _srcName = srcName;
        _updateInterval = updateInterval < 0 ? J4JMapControl.DefaultUpdateEventInterval : updateInterval;
        _logger = loggerFactory?.CreateLogger<MapPositions>();
    }

    public string? LatitudeProperty
    {
        get => _latProp;

        set
        {
            _latProp = value;
            InitializeSource();
        }
    }

    public string? LongitudeProperty
    {
        get => _longProp;

        set
        {
            _longProp = value;
            InitializeSource();
        }
    }

    public string? LatLongProperty
    {
        get => _latLongProp;

        set
        {
            _latLongProp = value;
            InitializeSource();
        }
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

    public List<ValidationItem> ProcessedItems { get; private set; } = new();

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
        switch( _srcValidator.Validate( Source, out var processed ) )
        {
            case DataSourceValidationResult.SourceNotEnumerable:
                _logger?.LogWarning("Data source {source} is not enumerable", _srcName);
                return;

            case DataSourceValidationResult.UndefinedSource:
                _logger?.LogWarning("Data source {source} is not defined", _srcName);
                return;

            case DataSourceValidationResult.Unprocessed:
                _logger?.LogWarning("Data source {source} was not validated", _srcName);
                return;

            case DataSourceValidationResult.Processed:
                // no op; proceed, but warn of oddities
                if( processed.Any(x=>x.ValidationResults.Any(y=>y.Value != DataItemValidationResult.Validated)))
                    _logger?.LogWarning("Data source {source} was validated but errors were found", _srcName);

                break;
        }

        // decouple property changed event handler from existing items
        foreach (var validationItem in ProcessedItems )
        {
            if (validationItem.Item is INotifyPropertyChanged propChanged)
                propChanged.PropertyChanged -= ItemPropertyChanged;
        }

        // set up property changed event handler for valid items
        foreach (var validationItem in processed)
        {
            if (validationItem.Item is not INotifyPropertyChanged propChanged)
                continue;

            propChanged.PropertyChanged += ItemPropertyChanged;
        }

        ProcessedItems = processed;
    }

    public List<GeoData> GetGeoData()
    {
        var retVal = new List<GeoData>();

        foreach( var item in ProcessedItems )
        {
            var newGeo = new GeoData( item );

            // specific Latitude and Longitude properties override LatLong properties
            var itemType = item.GetType();

            var latText = string.IsNullOrEmpty( LatitudeProperty )
                ? string.Empty
                : (string?) itemType.GetProperty( LatitudeProperty )!.GetValue( item );

            var longText = string.IsNullOrEmpty(LongitudeProperty)
                ? string.Empty
                : (string?) itemType.GetProperty(LongitudeProperty)!.GetValue(item);

            if( !string.IsNullOrEmpty( latText )
            && MapExtensions.TryParseToLatitude( latText, out var latitude )
            && !string.IsNullOrEmpty( longText )
            && MapExtensions.TryParseToLongitude( longText, out var longitude ) )
            {
                newGeo.Latitude = latitude;
                newGeo.Longitude = longitude;
                newGeo.LocationIsValid = true;
            }
            else
            {
                var latLongText = string.IsNullOrEmpty( LatLongProperty )
                    ? string.Empty
                    : (string?) itemType.GetProperty( LatLongProperty )!.GetValue( item );

                if( MapExtensions.TryParseToLatLong( latLongText, out latitude, out longitude ) )
                {
                    newGeo.Latitude = latitude;
                    newGeo.Longitude = longitude;
                    newGeo.LocationIsValid = true;
                }
            }

            retVal.Add( newGeo );
        }

        return retVal;
    }

    private void ItemPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        _throttleItemChange.Throttle(_updateInterval, _ =>
        {
            SourceUpdated?.Invoke( this, EventArgs.Empty );
        });
}
