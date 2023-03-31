using System;
using System.ComponentModel;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;
using Microsoft.UI.Xaml;

namespace J4JSoftware.J4JMapWinLibrary;

public sealed partial class J4JMapControl
{
    public string MapProjection
    {
        get => (string) GetValue( MapProjectionProperty );
        set => SetValue( MapProjectionProperty, value );
    }

    public string MapStyle
    {
        get => (string) GetValue( MapStyleProperty );
        set => SetValue( MapStyleProperty, value );
    }

    private void UpdateProjection()
    {
        if( !_cacheIsValid )
            UpdateCaching();

        var cache = _tileMemCache ?? _tileFileCache;

        var projResult = _projFactory.CreateProjection( MapProjection, cache );
        if( !projResult.ProjectionTypeFound )
        {
            J4JDeusEx.OutputFatalMessage( $"Could not create projection '{MapProjection}'", _logger );
            throw new InvalidOperationException( $"Could not create projection '{MapProjection}'" );
        }

        if( !projResult.Authenticated )
        {
            J4JDeusEx.OutputFatalMessage( $"Could not authenticate projection '{MapProjection}'", _logger );
            throw new InvalidOperationException( $"Could not authenticate projection '{MapProjection}'" );
        }

        _projection = projResult.Projection!;
        _projection.LoadComplete += (_, _) => _dispatcherQueue.TryEnqueue(LoadMapImages);

        if (!ConverterExtensions.TryParseToLatLong(Center, out var latitude, out var longitude))
            _logger.Error("Could not parse Center ('{0}') to latitude/longitude, defaulting to 0/0", Center);

        if (MapRegion != null)
        {
            MapRegion.ConfigurationChanged -= MapRegionConfigurationChanged;
            MapRegion.BuildUpdated -= MapRegionBuildUpdated;
        }

        MapRegion = new MapRegion( _projection, _logger )
                   .Center( latitude, longitude )
                   .Scale( (int) MapScale )
                   .Heading( (float) Heading )
                   .Size( (float) Height, (float) Width );

        if( _movementProcessor != null )
        {
            _movementProcessor.Rotated -= OnRotationHint;
            _movementProcessor.RotationsStarted -= OnRotationHintsStarted;
            _movementProcessor.RotationsEnded -= OnRotationHintsEnded;
        }

        _movementProcessor = new MovementProcessor( this, _logger );
        _movementProcessor.Rotated += OnRotationHint;
        _movementProcessor.RotationsStarted += OnRotationHintsStarted;
        _movementProcessor.RotationsEnded += OnRotationHintsEnded;

        MapRegion.ConfigurationChanged += MapRegionConfigurationChanged;
        MapRegion.BuildUpdated += MapRegionBuildUpdated;

        MinScale = _projection.MinScale;
        MaxScale = _projection.MaxScale;

        MapRegion.Build();
    }

    private void MapRegionBuildUpdated(object? sender, RegionBuildResults e)
    {
        switch (e.Change)
        {
            case MapRegionChange.Empty:
            case MapRegionChange.NoChange:
                break;

            case MapRegionChange.OffsetChanged:
                SetImagePanelTransforms(e);
                break;

            case MapRegionChange.LoadRequired:
                _projection!.LoadRegionAsync(MapRegion!);
                SetImagePanelTransforms(e);
                break;

            default:
                throw new InvalidEnumArgumentException( $"Unsupported {typeof( MapRegionChange )} value '{e.Change}'" );
        }
    }

    private void MapRegionConfigurationChanged(object? sender, EventArgs e)
    {
        _throttleRegionChanges.Throttle(UpdateEventInterval, _ => MapRegion!.Build());
    }
}
