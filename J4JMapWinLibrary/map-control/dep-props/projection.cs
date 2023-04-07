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

            if( _projFactory != null && !_projFactory.InitializeFactory() )
                _logger?.LogError( "Projection factory failed to find projection classes" );
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

        var cache = _tileMemCache ?? _tileFileCache;

        var projResult = ProjectionFactory.CreateProjection( MapProjection, cache );
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
