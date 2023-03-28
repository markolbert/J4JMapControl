using System;
using J4JSoftware.DeusEx;
using J4JSoftware.J4JMapLibrary;
using J4JSoftware.J4JMapLibrary.MapRegion;

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
        _projection.LoadComplete += ( _, _ ) => _dispatcherQueue.TryEnqueue( LoadMapImages );

        if( !ConverterExtensions.TryParseToLatLong( Center, out var latitude, out var longitude ) )
            _logger.Error( "Could not parse Center ('{0}') to latitude/longitude, defaulting to 0/0", Center );

        MapRegion = new MapRegion( _projection, _logger );
        MapRegion.ConfigurationChanged += MapRegionConfigurationChanged;
        MapRegion.BuildUpdated += MapRegionBuildUpdated;

        MinScale = _projection.MinScale;
        MaxScale = _projection.MaxScale;

        MapRegion.Build();
    }

    private void MapRegionBuildUpdated( object? sender, BuildUpdatedArgument e )
    {
        switch( e.Change )
        {
            case MapRegionChange.NoChange:
                break;

            case MapRegionChange.OffsetChanged:
                SetImagePanelTransforms(e);
                InvalidateArrange();
                break;

            default:
                OnMapRegionLoaded(e);
                InvalidateMeasure();
                break;
        }
    }

    private void MapRegionConfigurationChanged( object? sender, EventArgs e )
    {
        _throttleRegionChanges.Throttle( UpdateEventInterval, _ => MapRegion!.Build() );
    }
}
