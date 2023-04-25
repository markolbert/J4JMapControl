using System;
using J4JSoftware.WindowsUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using J4JSoftware.J4JMapLibrary;

namespace J4JSoftware.J4JMapWinLibrary;

public abstract class MapPositions<TBindingSource> : DependencyObject
    where TBindingSource : class
{
    public record ErrorEventArgs( LogLevel Level, string Message );

    public event EventHandler? SourceUpdated;
    public event EventHandler<ErrorEventArgs>? Error;

    private readonly ThrottleDispatcher _throttleSourceChange = new();
    private readonly ThrottleDispatcher _throttleItemChange = new();
    private readonly int _updateInterval;
    private readonly string _srcName;

    private object? _source;
    private string? _latProp;
    private string? _longProp;
    private string? _latLongProp;

    protected MapPositions(
        TBindingSource bindingSource,
        string srcName,
        int updateInterval = J4JMapControl.DefaultUpdateEventInterval
    )
    {
        BindingSource = bindingSource;
        DataSourceValidator = new DataSourceValidator<TBindingSource>( bindingSource );
        _srcName = srcName;
        _updateInterval = updateInterval < 0 ? J4JMapControl.DefaultUpdateEventInterval : updateInterval;
    }

    public TBindingSource BindingSource { get; }
    public DataSourceValidator<TBindingSource> DataSourceValidator { get; }

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
        }
    }

    public List<ValidationItem> ProcessedItems { get; private set; } = new();
    public List<ValidationItem> ValidItems { get; private set; } = new();
    public List<IPlacedItem> PlacedItems { get; private set; } = new();

    private void SourceCollectionChanged(
        object? sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e
    )
    {
        _throttleSourceChange.Throttle( _updateInterval,
                                        _ =>
                                        {
                                            InitializeSource();
                                        } );
    }

    private void InitializeSource()
    {
        switch( DataSourceValidator.Validate( Source, out var processed ) )
        {
            case DataSourceValidationResult.SourceNotEnumerable:
                Error?.Invoke(this, new ErrorEventArgs(LogLevel.Warning, $"Data source {_srcName} is not enumerable")  );
                return;

            case DataSourceValidationResult.UndefinedSource:
                Error?.Invoke(this, new ErrorEventArgs(LogLevel.Warning, $"Data source {_srcName} is not defined"));
                return;

            case DataSourceValidationResult.Unprocessed:
                Error?.Invoke(this, new ErrorEventArgs(LogLevel.Warning, $"Data source {_srcName} was not validated"));
                return;

            case DataSourceValidationResult.Processed:
                // no op; proceed, but warn of oddities
                if( processed.Any(x=>x.ValidationResults.Any(y=>y.Value != DataItemValidationResult.Validated)))
                    Error?.Invoke(this, new ErrorEventArgs(LogLevel.Information, $"Data source {_srcName} was validated but errors were found"));

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

        ValidItems = processed.Where( x => x.Item != null && ItemIsValid( x ) )
                              .ToList();

        CreatePlacedItems();

        SourceUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void ItemPropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        _throttleItemChange.Throttle(_updateInterval, _ =>
        {
            InitializeSource();
        });

    protected abstract bool ItemIsValid( ValidationItem validationItem );
    internal abstract IPlacedItemInternal? CreatePlacedItem( ValidationItem validationItem );

    private void CreatePlacedItems()
    {
        PlacedItems.Clear();

        for( var idx = 0; idx < ValidItems.Count; idx++ )
        {
            var validatedItem = ValidItems[ idx ];
            var dataItem = validatedItem.Item;

            if( dataItem == null )
            {
                Error?.Invoke(this, new ErrorEventArgs(LogLevel.Warning, $"Undefined data for item# {idx} in {_srcName}, no visual element created"));
                continue;
            }

            var placedItem = CreatePlacedItem( validatedItem );
            if( placedItem == null )
            {
                Error?.Invoke( this,
                               new ErrorEventArgs( LogLevel.Warning,
                                                   $"Failed to create {typeof( IPlacedItem )} for item# {idx} in {_srcName}" ) );
                continue;
            }

            // specific Latitude and Longitude properties override LatLong properties
            var itemType = dataItem.GetType();

            var latText = string.IsNullOrEmpty( LatitudeProperty )
                ? string.Empty
                : (string?) itemType.GetProperty( LatitudeProperty )!.GetValue( dataItem );

            var longText = string.IsNullOrEmpty( LongitudeProperty )
                ? string.Empty
                : (string?) itemType.GetProperty( LongitudeProperty )!.GetValue( dataItem );

            if( !string.IsNullOrEmpty( latText )
            && MapExtensions.TryParseToLatitude( latText, out var latitude )
            && !string.IsNullOrEmpty( longText )
            && MapExtensions.TryParseToLongitude( longText, out var longitude ) )
                placedItem.Initialize( dataItem, latitude, longitude, true );
            else
            {
                var latLongText = string.IsNullOrEmpty( LatLongProperty )
                    ? string.Empty
                    : (string?) itemType.GetProperty( LatLongProperty )!.GetValue( dataItem );

                if( MapExtensions.TryParseToLatLong( latLongText, out latitude, out longitude ) )
                    placedItem.Initialize( dataItem, latitude, longitude, true );
            }

            if( ( (IPlacedItem) placedItem ).LocationIsValid )
                PlacedItems.Add( ( (IPlacedItem) placedItem ) );
        }
    }
}
